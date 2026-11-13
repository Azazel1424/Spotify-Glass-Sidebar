# Spotify Glass Sidebar v1.0.0

Pierwsze udostępnione wydanie aplikacji dla **Windows 11 x64**.

### Funkcje

- Zwinięty pasek oraz rozszerzony panel z okładką.
- Sterowanie lokalną sesją Spotify: play/pauza, poprzedni i następny utwór, przewijanie.
- Wyszukiwanie, przejście do biblioteki Spotify i lokalne ulubione.
- Zapis położenia, ustawień wyglądu, ikona zasobnika systemowego.
- Uruchamianie bez instalowania oddzielnego .NET Runtime.

### Instalacja

Pobierz `SpotifyGlassSidebar-Windows11-x64-v1.0.0.zip`, rozpakuj do zapisywalnego katalogu i uruchom `app\SpotifyGlassSidebar.exe`. Wymaga desktopowej aplikacji Spotify. To aplikacja przenośna, nie instalator.

### Testy i ograniczenia

Według raportu dostarczonego z wersją 1.0: 8/8 self-testów zaliczonych na Windows 11 x64 oraz pozytywne testy sterowania Spotify. Nie przetestowano m.in. Spotify Free, Spotify Connect i wszystkich konfiguracji monitorów. Pełny raport w `docs/TESTY.md` repozytorium.

**Ważne:** binarium zbudowano na .NET 9, dla którego wsparcie zakończyło się 10 listopada 2026 r. Zalecana aktualizacja do .NET 10 LTS w kolejnym wydaniu.

**Projekt nieoficjalny, niezwiązany ze Spotify.** EXE nie jest podpisany cyfrowo.