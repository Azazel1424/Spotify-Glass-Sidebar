# Spotify Glass Sidebar

**Spotify Glass Sidebar** to lekki, przenośny widget muzyczny dla **Windows 11 x64**. Łączy szklany wygląd inspirowany stylem Acrylic z obsługą muzyki odtwarzanej w desktopowej aplikacji Spotify.

> **Projekt nieoficjalny.** Spotify Glass Sidebar jest niezależną aplikacją i nie jest tworzony, sponsorowany ani wspierany przez Spotify.

## Podgląd

| Rozwinięty panel | Zwinięty pasek |
| :---: | :---: |
| ![Rozwinięty panel Spotify Glass Sidebar](screenshots/podglad-panel.jpg) | ![Zwinięty pasek Spotify Glass Sidebar](screenshots/podglad-pasek.jpg) |

## Najważniejsze funkcje

- **Dwa widoki:** pełny panel i minimalistyczny pasek boczny.
- **Informacje o utworze:** tytuł, wykonawca, okładka, czas i stan odtwarzania.
- **Sterowanie:** odtwarzanie/pauza, poprzedni i następny utwór oraz przewijanie suwakiem — w zakresie obsługiwanym przez Spotify.
- **Wyszukiwanie i biblioteka:** otwieranie odpowiednich widoków w Spotify.
- **Ulubione widgetu:** lokalne zakładki do utworów, niezależne od polubionych utworów na koncie Spotify.
- **Personalizacja:** przyciemnienie szkła, opcja „zawsze na wierzchu” i animacja odtwarzania.
- **Wygodna obsługa:** przeciąganie okna, zwijanie, ikona w zasobniku systemowym i zapamiętywanie ustawień.
- **Bez logowania:** aplikacja nie wymaga podawania hasła do Spotify ani tokenu Spotify Web API.

## Wymagania

- Windows 11 **x64**.
- Zainstalowana **desktopowa aplikacja Spotify** z aktywną lokalną sesją odtwarzania.
- Nie trzeba instalować **Rainmetera** ani osobno środowiska .NET — wydanie jest publikowane jako aplikacja samodzielna (`win-x64`).

**Ważne:** samo odtwarzanie Spotify w przeglądarce nie jest obsługiwane jako źródło sesji. Efekt Acrylic najlepiej działa z włączonymi efektami przezroczystości systemu Windows 11; natywne tło systemowe dostępne jest od Windows 11 22H2.

## Instalacja

1. Pobierz paczkę ZIP z sekcji **Releases** tego repozytorium.
2. Wypakuj **całą paczkę** do zapisywalnego folderu, np. `Dokumenty\SpotifyGlassSidebar`.
3. Uruchom `app\SpotifyGlassSidebar.exe`.
4. Włącz Spotify na komputerze i rozpocznij odtwarzanie utworu. Widget powinien rozpoznać sesję automatycznie.

Plik EXE nie jest obecnie podpisany certyfikatem wydawcy, dlatego Windows może wyświetlić ostrzeżenie o nieznanym wydawcy. Pobieraj aplikację wyłącznie z zaufanego wydania.

## Obsługa

| Czynność | Jak wykonać |
| --- | --- |
| Zwiń panel | Strzałka w prawym górnym rogu lub `Ctrl+B` w aktywnym oknie |
| Rozwiń pasek | Kliknij zielone logo na zwiniętym pasku |
| Przenieś okno | Przeciągnij pusty fragment nagłówka; na pasku obszar nad lub pod logo |
| Steruj muzyką | Przyciski pod okładką i suwak odtwarzania |
| Wyszukaj utwór | **Szukaj**, wpisz frazę, naciśnij `Enter` |
| Zamknij wyszukiwanie/ustawienia | `Esc` |
| Ukryj/przywróć okno | Ikona aplikacji w zasobniku systemowym |
| Zamknij aplikację | Ustawienia, menu zasobnika lub `Alt+F4` |

Ustawienia, pozycja okna, tryb paska i lokalne ulubione zapisywane są w `app\data\settings.json`. Przenosząc konfigurację na inny komputer, skopiuj również folder `data`. Autostart można skonfigurować ręcznie przez skrót do EXE w folderze Autostart systemu Windows.

## Ograniczenia

- Widget steruje **lokalną sesją Spotify** poprzez mechanizmy multimedialne Windows. Dostępność poszczególnych poleceń zależy od Spotify.
- Spotify Connect na innym urządzeniu, reklamy, podcasty i treści na żywo mogą działać inaczej.
- „Ulubione widgetu” **nie synchronizują się** z polubionymi utworami konta Spotify.
- Animowane słupki sygnalizują odtwarzanie — **nie analizują dźwięku**.
- Jest to osobne okno, a nie element panelu widżetów Windows. Polecenie „Pokaż pulpit” może je ukryć; można je przywrócić z zasobnika.
- Według dokumentacji aplikacja **nie wysyła własnych żądań sieciowych**. Linki nawigacyjne otwierają Spotify lub odpowiednią stronę w przeglądarce.

## Testy wersji 1.0

Weryfikacja z **13 listopada 2026 r.** na Windows 11 x64 (kompilacja 26300):

- **8/8 testów automatycznych zakończonych powodzeniem**.
- Potwierdzono odczyt utworów, okładek i stanu odtwarzania oraz obsługę play/pauza, zmiany utworów i przewijania.
- Sprawdzono oba tryby widoku, zwijanie/rozwijanie, zapisywanie ustawień i przywracanie położenia okna.
- Nie wszystkie warianty zostały przetestowane, m.in. Spotify Free, Spotify Connect, reklamy oraz różne konfiguracje wielu monitorów i DPI.

Szczegóły: [Raport testów](docs/TESTY.md) · [Wyniki self-test](docs/self-test.txt).

## Technologia

- **C# / WPF / .NET 9**
- Publikacja: **Windows x64**, samodzielna aplikacja z dołączonym środowiskiem .NET
- Integracja multimedialna: [Windows GlobalSystemMediaTransportControls](https://learn.microsoft.com/en-us/uwp/api/windows.media.control.globalsystemmediatransportcontrolssessionmanager)
- Efekt szkła: [DWM_SYSTEMBACKDROP_TYPE](https://learn.microsoft.com/en-us/windows/win32/api/dwmapi/ne-dwmapi-dwm_systembackdrop_type)

Dodatkowe informacje: [Instrukcja użytkowania](docs/INSTRUKCJA.md).

## Znaki towarowe

Nazwa Spotify, logo Spotify i pozostałe znaki towarowe należą do ich właścicieli. Projekt jest niezależny i nieoficjalny.
