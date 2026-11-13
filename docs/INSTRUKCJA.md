# Spotify Glass Sidebar

Przenośny widget dla **Windows 11 x64**, inspirowany dostarczonymi zrzutami. Niezależny projekt, nieoficjalny produkt Spotify.

## Uruchomienie

1. Wypakuj cały ZIP do własnego, zapisywalnego folderu, np. `Dokumenty\SpotifyGlassSidebar`.
2. Otwórz `app\SpotifyGlassSidebar.exe`. Program ma dołączone środowisko .NET; nie musisz instalować SDK ani Rainmetera.
3. Uruchom desktopową aplikację Spotify i włącz dowolny utwór. Widget sam odnajdzie sesję; po pauzie nadal pokazuje utwór.

Widget nie prosi o hasło, klucz API ani logowanie. Steruje Spotify na tym komputerze. Otwarta wyłącznie strona Spotify w przeglądarce nie jest obsługiwana jako źródło odtwarzania.

## Obsługa

- **Przeciąganie:** złap puste miejsce w nagłówku. Na wąskim pasku złap miejsce nad lub pod zielonym logo.
- **Zwijanie:** strzałka w prawym górnym rogu; rozwijanie — zielone logo wąskiego paska. Skrót w aktywnym oknie: `Ctrl+B`.
- **Muzyka:** play/pauza, poprzedni i następny utwór. Kliknij lub przeciągnij suwak, aby przewinąć. Działają tylko polecenia udostępnione przez Spotify.
- **Serce:** lokalna zakładka do bieżącego utworu. Lista „Ulubione widgetu” nie jest listą polubionych na koncie Spotify. Kliknięcie zakładki wyszukuje utwór w Spotify.
- **Szukaj:** wpisz tytuł, wykonawcę lub playlistę i naciśnij Enter. Wyniki otwierają się w Spotify.
- **Twoja biblioteka:** otwiera bibliotekę w Spotify.
- **Ustawienia:** zawsze na wierzchu, przyciemnienie szkła, animacja odtwarzania, ukrycie i zamknięcie. `Esc` zamyka panel ustawień lub wyszukiwania.
- **Zasobnik systemowy:** zielona ikona przy zegarze (ewentualnie pod strzałką ukrytych ikon). Dwuklik przywraca widget; prawy przycisk otwiera menu.
- **Zamknięcie:** Ustawienia → Zamknij widget, menu ikony w zasobniku lub `Alt+F4`.

Pozycja, tryb paska, ustawienia i lokalne ulubione zapisują się w `app\data\settings.json`. Aby przenieść je na inny komputer, skopiuj również folder `data`.

## Skrót na pulpicie i autostart

Możesz utworzyć standardowy skrót Windows do pliku EXE. Opcjonalnie skopiuj taki skrót do swojego folderu Autostart. Aplikacja sama nie włącza autostartu ani nie zmienia ustawień systemowych.

## Zachowanie i ograniczenia

- To osobne, bezramkowe okno. Nie jest dodatkiem do panelu widżetów Windows ani częścią powłoki pulpitu. Polecenie Windows „Pokaż pulpit” może je schować; przywróć je ikoną w zasobniku.
- Efekt Acrylic używa systemowych efektów przezroczystości. W Windows 11 22H2 i nowszych jest dostępny natywny backdrop. Na starszym Windows 11 lub przy wyłączonych efektach tło może być jednolite.
- Animowane słupki wskazują odtwarzanie; nie analizują dźwięku.
- Kontrola dotyczy lokalnej sesji Spotify. Odtwarzanie na telefonie/głośniku przez Spotify Connect, reklamy, treści na żywo i różne konta mogą udostępniać inne polecenia. Nie są wymagane ani pobierane dane konta przez Spotify Web API.
- Brak sesji: uruchom desktopowe Spotify i rozpocznij odtwarzanie. Odświeżanie trwa zwykle do 2 sekund.
- Plik EXE nie jest podpisany certyfikatem wydawcy. Kod źródłowy i skrypt kompilacji są dostępne w repozytorium GitHub.

## Informacje techniczne

WPF / .NET 9, publikacja samodzielna `win-x64`. Odczyt i sterowanie: [Windows GlobalSystemMediaTransportControls](https://learn.microsoft.com/en-us/uwp/api/windows.media.control.globalsystemmediatransportcontrolssessionmanager). Szkło: [DWM_SYSTEMBACKDROP_TYPE](https://learn.microsoft.com/en-us/windows/win32/api/dwmapi/ne-dwmapi-dwm_systembackdrop_type).

Widget nie wysyła własnych żądań sieciowych. Przyciski nawigacji otwierają URI Spotify, a gdy nie ma zarejestrowanej aplikacji — odpowiednią stronę Spotify w przeglądarce.