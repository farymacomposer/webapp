'use client'
import React, {useEffect, useState} from "react";
import {CreateReviewOrderRequest, OrderPositionDto, OrderQueueApi, ReviewOrderApi, ReviewOrderType} from "@/api/rest";

export default function StreamSpace() {
    const [orders, setOrders] = useState<OrderPositionDto[]>([]);
    const [loading, setLoading] = useState(false);
    const [formData, setFormData] = useState<CreateReviewOrderRequest>({
        nickname: '',
        orderType: 'Unspecified',
        trackUrl: '',
        paymentAmount: 0,
        userComment: ''
    });

    const reviewOrderApi = new ReviewOrderApi();
    const orderQueueApi = new OrderQueueApi();

    // Загрузка очереди заказов
    const loadQueue = async () => {
        try {
            const response = await orderQueueApi.apiOrderQueueGetOrderQueueGet();
            setOrders(response.data.activeOrders || []);
        } catch (error) {
            console.error('Ошибка загрузки очереди:', error);
            alert('Не удалось загрузить очередь заказов');
        }
    };

    useEffect(() => {
        loadQueue();
    }, []);

    // 1. Создание заказа (Взятие заказа в работу)
    const takeOrder = async () => {
        if (!formData.nickname.trim()) {
            alert('Введите псевдоним');
            return;
        }

        setLoading(true);

        try {
            const payload: CreateReviewOrderRequest = {
                nickname: formData.nickname,
                orderType: 'Unspecified',
                trackUrl: formData.trackUrl || null,
                paymentAmount: formData.paymentAmount,
                userComment: formData.userComment || null
            };

            const response = await reviewOrderApi.apiReviewOrderCreateReviewOrderPost('', payload);

            console.log('Заказ создан:', response.data);
            alert(`Заказ успешно создан! ID: ${response.data.reviewOrder.id}`);

            setFormData({
                nickname: '',
                orderType: 'Unspecified',
                trackUrl: '',
                paymentAmount: 0,
                userComment: ''
            });

            await loadQueue();
        } catch (error) {
            console.error('Ошибка создания заказа:', error);
            alert('Не удалось создать заказ');
        } finally {
            setLoading(false);
        }
    };

    // 2. Получение текущего состояния очереди
    const refreshQueue = async () => {
        setLoading(true);
        await loadQueue();
        setLoading(false);
    };

    // 3. Выполнение заказа
    const completeOrder = async (orderId: number) => {
        if (!confirm('Вы уверены, что хотите завершить этот заказ?')) {
            return;
        }

        setLoading(true);
        try {
            await reviewOrderApi.apiReviewOrderCompleteReviewOrderPost({reviewOrderId: orderId, rating: 10});
            alert('Заказ успешно выполнен!');
            await loadQueue();
        } catch (error) {
            console.error('Ошибка выполнения заказа:', error);
            alert('Не удалось выполнить заказ');
        } finally {
            setLoading(false);
        }
    };

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
        const {name, value} = e.target as HTMLInputElement;
        setFormData(prev => ({
            ...prev,
            [name]: value
        }));
    };

    return (
        <div>
            <section>
                <h2>Взятие заказа в работу</h2>
                <div>
                    <div>
                        <label>
                            Псевдоним *
                        </label>
                        <input
                            type="text"
                            name="nickname"
                            value={formData.nickname}
                            onChange={handleInputChange}
                            maxLength={40}
                        />
                    </div>

                    <div>
                        <label>
                            Тип заказа *
                        </label>
                        <select
                            name="orderType"
                            value={formData.orderType}
                            onChange={handleInputChange}
                        >
                            <option value="Unspecified">Не указан</option>
                            <option value="Standard">Стандартный</option>
                            <option value="Premium">Премиум</option>
                            <option value="Express">Экспресс</option>
                        </select>
                    </div>

                    <div>
                        <label>
                            Ссылка на трек
                        </label>
                        <input
                            type="url"
                            name="trackUrl"
                            value={formData.trackUrl}
                            onChange={handleInputChange}
                            placeholder="https://..."
                        />
                    </div>

                    <div>
                        <label>
                            Сумма платежа
                        </label>
                        <input
                            type="number"
                            name="paymentAmount"
                            value={formData.paymentAmount}
                            onChange={handleInputChange}
                            step="0.01"
                            min="0"
                            placeholder="0.00"
                        />
                    </div>

                    <div>
                        <label>
                            Комментарий пользователя
                        </label>
                        <textarea
                            name="userComment"
                            value={formData.userComment}
                            onChange={handleInputChange}
                            maxLength={200}
                            rows={4}
                            placeholder="Дополнительные пожелания..."
                        />
                    </div>

                    <button
                        onClick={takeOrder}
                        disabled={loading}
                    >
                        {loading ? 'Обработка...' : 'Создать заказ'}
                    </button>
                </div>
            </section>

            {/* Текущая очередь заказов */}
            <section>
                <div>
                    <h2>Текущее состояние очереди заказов</h2>
                    <button
                        onClick={refreshQueue}
                        disabled={loading}
                    >
                        {loading ? 'Загрузка...' : 'Обновить очередь'}
                    </button>
                </div>

                {orders.length === 0 ? (
                    <p>Очередь пуста</p>
                ) : (
                    <div>
                        {orders.map((order, index) => (
                            <div key={order.order.id}>
                                <div>
                                    <h3>
                                        #{index + 1} - {order.order.mainNickname}
                                    </h3>
                                    <p>
                                        <strong>Тип:</strong> {order.order.categoryType}
                                    </p>
                                    {order.order.trackUrl && (
                                        <p>
                                            <strong>Трек:</strong>{' '}
                                            <a href={order.order.trackUrl} target="_blank" rel="noopener noreferrer">
                                                {order.order.trackUrl}
                                            </a>
                                        </p>
                                    )}
                                    {order.order.totalAmount && (
                                        <p>
                                            <strong>Сумма:</strong> {order.order.totalAmount} ₽
                                        </p>
                                    )}
                                    {order.order.userComment && (
                                        <p>
                                            <strong>Комментарий:</strong> {order.order.userComment}
                                        </p>
                                    )}
                                    <p>
                                        <strong>Статус:</strong> {order.order.status}
                                    </p>
                                </div>
                                <button
                                    onClick={() => completeOrder(order.order.id)}
                                    disabled={loading}
                                >
                                    Выполнить
                                </button>
                            </div>
                        ))}
                    </div>
                )}
            </section>
        </div>
    );
}
