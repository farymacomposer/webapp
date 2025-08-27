using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;
using Faryma.Composer.Infrastructure.Models;

namespace DataLoader
{
    static class Program
    {
        static readonly UnitOfWork? uow;
        static readonly AppDbContext? _context;

        public static async Task<UserNickname> GetOrCreate(string nickname)
        {
            UserNickname? result = await uow.UserNicknameRepository.Find(nickname);

            if (result is null)
            {
                result = uow.UserNicknameRepository.Create(nickname);
                uow.UserAccountRepository.Create(result);
                await uow.SaveChanges();
            }

            return result;
        }

        public static async Task<ComposerStream> GetComposerStream(DateOnly eventDate)
        {
            return _context.Add(new ComposerStream
            {
                EventDate = eventDate,
                Status = ComposerStreamStatus.Completed,
                Type = type
            }).Entity;
        }

        static async Task Main(string[] args)
        {
            Data? data = null;

            UserNickname userNickname = await GetOrCreate(data.Nickname);
            ComposerStream creationStream = await GetComposerStream();
            ComposerStream processingStream = await GetComposerStream();

            Transaction deposit = uow.TransactionRepository.CreateDeposit(userNickname.Account, command.PaymentAmount);
            Transaction payment = uow.TransactionRepository.CreatePayment(userNickname.Account, command.PaymentAmount);

            //CreateDonation
            //CreateFree
            //CreateReview
        }

        static ReviewOrder CreateDonation(
    ComposerStream stream,
    Transaction transaction,
    int nominalAmount,
    string? trackUrl,
    string? userComment)
        {
            return _context.Add(new ReviewOrder
            {
                CreatedAt = DateTime.UtcNow,
                IsFrozen = false,
                Type = ReviewOrderType.Donation,
                CategoryType = OrderCategoryType.Unspecified,
                Status = (trackUrl is null) ? ReviewOrderStatus.Preorder : ReviewOrderStatus.Pending,
                MainNickname = transaction.Account.UserNickname.Nickname,
                MainNormalizedNickname = transaction.Account.UserNickname.NormalizedNickname,
                TrackUrl = trackUrl,
                UserComment = userComment,
                CreationStream = stream,
                UserNicknames = { transaction.Account.UserNickname },
                NominalAmount = nominalAmount,
                Payments = { transaction },
            }).Entity;
        }

        static ReviewOrder CreateFree(
            ComposerStream stream,
            UserNickname userNickname,
            int nominalAmount,
            string? trackUrl,
            string? userComment,
            ReviewOrderType type)
        {
            return _context.Add(new ReviewOrder
            {
                CreatedAt = DateTime.UtcNow,
                IsFrozen = false,
                Type = type,
                CategoryType = OrderCategoryType.Unspecified,
                Status = (trackUrl is null) ? ReviewOrderStatus.Preorder : ReviewOrderStatus.Pending,
                MainNickname = userNickname.Nickname,
                MainNormalizedNickname = userNickname.NormalizedNickname,
                TrackUrl = trackUrl,
                UserComment = userComment,
                CreationStream = stream,
                UserNicknames = { userNickname },
                NominalAmount = nominalAmount,
            }).Entity;
        }

        static Review CreateReview(
    ReviewOrder inProgressOrder,
    int rating,
    DateTime createdAt)
        {
            return _context.Reviews.Add(new Review
            {
                ReviewOrder = inProgressOrder,
                RatingValue = rating,
                CreatedAt = createdAt,
                UpdatedAt = createdAt,
            }).Entity;
        }
    }

    public sealed record Data
    {
        /// <summary>
        /// Псевдонимы
        /// </summary>
        public required string[] UserNicknames { get; set; }

        public required DateOnly CreationStream_Date { get; set; }
        public required ComposerStreamType CreationStream_Type { get; set; }
        public required DateTime CreationStream_WentLiveAt { get; set; }
        public required DateTime CreationStream_CompletedAt { get; set; }

        public required DateOnly ProcessingStream_Date { get; set; }
        public required ComposerStreamType ProcessingStream_Type { get; set; }
        public required DateTime ProcessingStream_WentLiveAt { get; set; }
        public required DateTime ProcessingStream_CompletedAt { get; set; }

        /// <summary>
        /// Сумма операции
        /// </summary>
        public required decimal Transaction_Amount { get; set; }

        /// <summary>
        /// Дата и время совершения операции
        /// </summary>
        public required DateTime Transaction_CreatedAt { get; set; }

        /// <summary>
        /// Дата и время создания заказа
        /// </summary>
        public DateTime ReviewOrder_CreatedAt => CreationStream_WentLiveAt;

        /// <summary>
        /// Дата и время взятия заказа в работу
        /// </summary>
        public required DateTime ReviewOrder_InProgressAt { get; set; }

        /// <summary>
        /// Дата и время выполнения заказа
        /// </summary>
        public required DateTime ReviewOrder_CompletedAt { get; set; }

        /// <summary>
        /// Тип заказа
        /// </summary>
        public required ReviewOrderType ReviewOrder_Type { get; set; }

        /// <summary>
        /// Тип категории заказа
        /// </summary>
        public required OrderCategoryType ReviewOrder_CategoryType { get; set; }

        /// <summary>
        /// Ссылка на трек
        /// </summary>
        public required string ReviewOrder_TrackUrl { get; set; }

        /// <summary>
        /// Номинальная стоимость заказа
        /// </summary>
        public required decimal ReviewOrder_NominalAmount { get; set; }

        /// <summary>
        /// Оценка
        /// </summary>
        public required int Review_RatingValue { get; set; }

        /// <summary>
        /// Дата и время создания разбора
        /// </summary>
        public DateTime Review_CreatedAt => ReviewOrder_CompletedAt;

        /// <summary>
        /// Дата и время добавления трека
        /// </summary>
        public DateTime Track_AddedAt => CreationStream_WentLiveAt;

        /// <summary>
        /// Название трека
        /// </summary>
        public required string Track_Title { get; set; }

        /// <summary>
        /// Ссылка на трек
        /// </summary>
        public string Track_Url => ReviewOrder_TrackUrl;

        /// <summary>
        /// Дата выпуска трека
        /// </summary>
        public required DateOnly Track_ReleaseDate { get; set; }

        /// <summary>
        /// Ссылка на обложку
        /// </summary>
        public required string Track_CoverUrl { get; set; }

        /// <summary>
        /// Расширенные жанры
        /// </summary>
        public required List<string> Track_ExtendedGenres { get; set; }

        /// <summary>
        /// Тэги
        /// </summary>
        public required List<TrackTag> Track_Tags { get; set; }

        /// <summary>
        /// Имя исполнителя
        /// </summary>
        public required string[] TrackArtist_Names { get; set; }

        /// <summary>
        /// Название страны
        /// </summary>
        public required string TrackCountry_Name { get; set; }

        /// <summary>
        /// Название жанра
        /// </summary>
        public required int[] TrackGenre_Ids { get; set; }
    }
}