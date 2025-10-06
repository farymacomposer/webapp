
## Обновлённая файловая структура

```
src/
  app/                    # страницы (роуты)
    catalog/              # страница каталога
    help/                 # страница справки
    stream-space/         # страница стрим-пространства
    layout.tsx            # общий layout (header, контейнер, wrapper)
    page.tsx              # главная страница
  components/             # UI и логические модули интерфейса
    layout/               # компоненты структуры страницы (header, main, footer)
    shared/               # переиспользуемые атомарные элементы (button, input)
    widgets/              # сложные виджеты, объединяющие несколько компонентов (card, streamWindow)
  config/                 # конфигурационные файлы проекта (routes, constants)
  lib/                    # служебные модули и вспомогательные функции
    api/                  # функции для работы с API (fetch, запросы)
    hooks/                # кастомные React-хуки (debounce, useFetch и т.п.)
    utils/                # утилиты и хелперы общего назначения
  styles/                 # глобальные и модульные стили
    components/           # стили, сгруппированные по компонентам
    global.css            # глобальные стили: сбросы, шрифты, переменные
  types/                  # глобальные TypeScript-типы
  data/mocks/             # хард-код для даты если необходимо
public/                   # статика: изображения, шрифты, логотипы, og-картинки
```

## Базовые соглашения

### Импорты
- В `tsconfig.json`:
  ```json
  { "compilerOptions": { "baseUrl": "src", "paths": { "@/*": ["*"] } } }
  ```
  Импортируй как `@/lib/api/tracks`.

### Именование
- Компоненты: `PascalCase`, один файл — один компонент по умолчанию.
- Классы в CSS-module: короткие — `root`, `inner`, `title`, `btn`.

## Как добавить **новую страницу**
1. Создай папку в `src/app/<route>/`.
2. Добавь `page.tsx`:
   ```tsx
   // src/app/faq/page.tsx 
   export default function FAQPage() {
     return <h1>FAQ</h1>;
   }
   ```

## Как добавить **новый компонент**
1. Создай файл в `src/components/<domain>/<Name>.tsx`.
2. Стили:
   `src/styles/components/<domain>/<Name>.module.css`.
3. Импорт стилей:
   ```tsx
   import style from "../../styles/components/<domain>/<Name>.module.css";
   export default function TrackCard(){ return <div className={style.root}>…</div>; }
   ```

## Чеклист при добавлении модуля
- [ ] Страница в `app/<route>/page.tsx`
- [ ] Компонент(ы) в `components/<domain>/…`
- [ ] Стили (CSS-module)  в `styles/components/<domain>/…`
- [ ] Клиентская функция в `lib/api/…`
- [ ] Типы в `types/` (или генерация из Swagger)
