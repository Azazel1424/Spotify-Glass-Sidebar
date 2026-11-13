# Weryfikacja wersji 1.0

Zakończona 13 listopada 2026. Windows 11 x64, kompilacja systemu 26300. Testy interfejsu wykonano w rzeczywistych oknach widgetu i desktopowego Spotify, przez computer-use. Parametr `--ui-test` pokazuje dodatkowo przycisk widgetu na pasku zadań, aby narzędzie dostępności mogło odnaleźć okno; normalny start używa ikony w zasobniku.

## PASS — kompilacja i testy automatyczne

- Publikacja Release / win-x64 / samodzielny EXE z dołączonym .NET.
- 8/8 testów: format czasu i niepoprawne wartości; kodowanie URI; wybór sesji Spotify zamiast innych aplikacji; brak pliku ustawień; zapis i ponowny odczyt ustawień, fizycznego położenia i lokalnych zakładek; uszkodzony JSON; puste/zdublowane zakładki i ograniczenie przyciemnienia; niepoprawne współrzędne.
- Brak pliku `error.log` po końcowych testach interfejsu.

## PASS — testy na uruchomionym Spotify

- Odczyt rzeczywistego tytułu, wykonawcy, okładki, czasu i statusu odtwarzania.
- Play i pauza: zmiana stanu Spotify oraz licznika odtwarzania.
- Następny utwór i powrót do poprzedniego. Przycisk poprzedniego utworu zachowuje zachowanie Spotify: może najpierw cofnąć bieżący utwór do początku.
- Kliknięcie suwaka: zmiana pozycji w Spotify, potwierdzona ponownym odczytem po kilku sekundach.
- Przeciągnięcie suwaka: potwierdzony zapis nowej pozycji w Spotify.
- Wyszukiwanie przekierowuje do wyników w aplikacji Spotify. Testowa fraza: Daft Punk. W testowanej wersji Spotify odstęp w URI był wyświetlany w polu wyszukiwania jako „+”; wyniki zostały zwrócone.
- Dodanie lokalnej zakładki sercem, zapis w JSON i wyświetlenie na liście. Testowa zakładka została usunięta z pliku po zamknięciu widgetu; zapisane wcześniej preferencje zachowano.

## PASS — okno i wygląd

- Wizualna kontrola pełnego panelu, wąskiego paska, ustawień, wyszukiwania i listy ulubionych.
- Zwijanie i rozwijanie, powrót z panelu klawiszem Escape.
- Przywrócenie po ukryciu do zasobnika przez ponowne uruchomienie EXE. Powraca pełny interfejs; nie powstaje druga kopia aplikacji.
- Zachowanie ustawień przyciemnienia i opcji zawsze na wierzchu po restarcie.
- Zachowanie fizycznego położenia na używanym monitorze po restarcie; sprawdzone również ujemne współrzędne ekranu. Położenie jest ograniczane do obszaru roboczego monitora.

## NOT TESTED — zakres poza obecną weryfikacją

- Spotify Free, reklamy, podcasty, Spotify Connect na innym urządzeniu.
- Odtwarzanie bez zainstalowanego Spotify, automatyczne ponowne połączenie po awarii Spotify.
- Wszystkie kombinacje monitorów, skalowania DPI i odłączania ekranów; starsze wersje Windows 11, wyłączone efekty przezroczystości.
- Bezpośredni przycisk biblioteki, usuwanie zakładki przez przycisk interfejsu, nawigacja suwakiem wyłącznie klawiaturą i dwuklik ikony zasobnika.
- Ręczne tworzenie skrótu i autostart. Aplikacja nie konfiguruje autostartu.

Nie są to testy certyfikacyjne Microsoft ani Spotify. Wersja jest przeznaczona do osobistego użytku na lokalnym komputerze.