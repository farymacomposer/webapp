
## Дерево проекта (основное)

```
src/
  app/                    # страницы (роуты)
    page.tsx              # главная
    catalog/page.tsx
    help/page.tsx
    stream-space/page.tsx
    layout.tsx            # общий layout (хедер/контейнер)
  components/             # UI и доменные модули
    header/               # Header, SearchBox, StatusLabel (+ их стили)
    stream/               # карточки стримов, очередь
    catalog/              # карточки треков, фильтры
    ui/                   # атомы: Button, Input, Modal и т.п.
  styles/
    global.css            # сбросы, шрифты, переменные
    components/           # стили модулей
  lib/
    http/                 # fetcher.ts (общая обёртка над fetch)
    api/                  # клиентские функции для UI: search и т.п.
    utils/                # общие утилиты
  hooks/                  # кастомные хуки (debounce и т.п.)
  config/                 # routes.ts (думаю нет необходимости, но наслучай если разрастётся проект может быть полезно)
  types/                  # TS-типы
  data/mocks/             # хард-код для даты если необходимо
public/                   # статика: шрифты, og-картинки, логотипы
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
