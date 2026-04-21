// Pripojujeme knihovny, ktere program potrebuje ke svemu chodu
using System;                     // Zakladni knihovna C# (Console, Math, atd.)
using System.Collections.Generic; // Potrebna pro pouziti List (seznam)
using System.IO;                  // Potrebna pro praci se soubory (cteni, zapis)
using System.Globalization;       // Potrebna aby cisla byla vzdy s teckou, ne s carkou

namespace TattooStudio
{
    // ============================================================
    // TRIDA Objednavka
    // Trida je jako "sablon" - popisuje co jedna objednavka obsahuje.
    // Stejne jako formular: ma policka pro jmeno, telefon, datum atd.
    // ============================================================
    class Objednavka
    {
        // Toto jsou "policka formulare" - data ktere kazda objednavka uchovava
        // "public" znamena ze k temto datum muze pristoupit i zbytek programu
        // "get; set;" znamena ze hodnotu lze cist i zapisovat
        public int Id { get; set; }                  // Cislo objednavky (1, 2, 3...)
        public string JmenoKlienta { get; set; }     // Jmeno a prijmeni klienta
        public string Telefon { get; set; }          // Telefonni cislo (9 cislic)
        public DateTime DatumNavstevy { get; set; }  // Datum navstevy (DateTime = specialni typ pro datum)
        public string PopisTetovani { get; set; }    // Popis motivu tetovani
        public double Cena { get; set; }             // Cena v korunach (double = cislo s desetinnou carkou)
        public bool Zaplaceno { get; set; }          // Je zaplaceno? (bool = pouze true nebo false)

        // ============================================================
        // METODA ToCsvRadek
        // Vezme data teto objednavky a slozi je do jednoho radku textu pro CSV soubor.
        // Vysledek vypada takto: 1;Jan Novak;603123456;15.08.2025;Drak;2500;false
        // ============================================================
        public string ToCsvRadek()
        {
            // Znak $ pred retezcem umoznuje vkladat promenne primo do textu pres {}
            // CultureInfo.InvariantCulture zajistuje ze cena bude vzdy s teckou (2500.50), ne s carkou
            return $"{Id};{JmenoKlienta};{Telefon};{DatumNavstevy:dd.MM.yyyy};{PopisTetovani};{Cena.ToString(CultureInfo.InvariantCulture)};{Zaplaceno}";
        }

        // ============================================================
        // METODA ZCsvRadku
        // Dostane jeden radek z CSV souboru a vytvori z nej objekt objednavky.
        // Dela opak nez ToCsvRadek - misto skladani rozebira.
        // "static" znamena ze tuto metodu muzeme volat bez vytvoreni objektu
        // ============================================================
        public static Objednavka ZCsvRadku(string radek)
        {
            // Split(';') - rozrizne retezec podle znaku ';' a vrati pole casti
            // Priklad: "1;Jan;603..." → ["1", "Jan", "603"...]
            string[] casti = radek.Split(';');

            // Vytvorime novou prazdnou objednavku a vyplnime jeji policka
            // Parse = prevod textu na jiny datovy typ (text "1" → cislo 1)
            // [0] = prvni prvek pole, [1] = druhy prvek atd. (zacina se od nuly!)
            return new Objednavka
            {
                Id = int.Parse(casti[0]),      // text "1" → cele cislo 1
                JmenoKlienta = casti[1],                 // text zustava textem
                Telefon = casti[2],                 // text zustava textem
                DatumNavstevy = DateTime.ParseExact(casti[3], "dd.MM.yyyy", CultureInfo.InvariantCulture), // text → datum
                PopisTetovani = casti[4],                 // text zustava textem
                Cena = double.Parse(casti[5], CultureInfo.InvariantCulture), // text → desetinne cislo
                Zaplaceno = bool.Parse(casti[6])      // text "false" → false
            };
        }
    }

    // ============================================================
    // HLAVNI TRIDA Program
    // Tady je ulozena veskera logika programu - menu, funkce, cteni souboru atd.
    // ============================================================
    class Program
    {
        // Nazev souboru kde se ukladaji data (soubor bude ve stejne slozce jako program)
        static string csouboru = "objednavky.csv";

        // Seznam vsech objednavek v pameti pocitace
        // List<Objednavka> = seznam objektu typu Objednavka (funguje jako pole, ale lze pridavat/mazat)
        static List<Objednavka> objednavky = new List<Objednavka>();

        // Pocitadlo pro generovani novych ID - zacina od 1
        static int dalsiId = 1;

        // ============================================================
        // METODA Main
        // Tato metoda se spusti jako uplne prvni po startu programu
        // ============================================================
        static void Main(string[] args)
        {
            // Nastavime UTF-8 aby se spravne zobrazovala ceska pismena (hacky, carky)
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Nacteme ulozena data ze souboru hned pri spusteni programu
            NacistData();

            // Ridici promenna hlavni smycky - true = program bezi, false = konec
            bool bezi = true;

            // Hlavni smycka programu - toci se dokud je bezi = true
            while (bezi)
            {
                // Zobrazime menu a cekame na volbu uzivatele
                ZobrazitMenu();

                // Precteme co uzivatel napsal na klavesnici
                // ?.Trim() = pokud uzivatel nic nezadal nevznikne chyba + odstrani mezery
                string volba = Console.ReadLine()?.Trim();

                // Switch = podle toho co uzivatel napsal zavolame spravnou funkci
                switch (volba)
                {
                    case "1": ZobrazitVsechny(); break; // "1" = zobraz vse
                    case "2": PridatObjednavku(); break; // "2" = pridej novou
                    case "3": OdstranitObjednavku(); break; // "3" = smaz
                    case "4": VyhledatKlienta(); break; // "4" = hledej
                    case "5": ZobrazitStatistiku(); break; // "5" = statistiky
                    case "6": OznacitZaplaceno(); break; // "6" = oznac jako zaplaceno
                    case "0":
                        UlozitData();                              // Uloz data do souboru
                        Console.WriteLine("\nData ulozena. Na shledanou!"); // Rozlouceni
                        bezi = false;                              // Zastav smycku = ukonci program
                        break;
                    default:                                       // Uzivatel zadal neco jineho
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\nNeplatna volba! Zadejte cislo 0-6.");
                        Console.ResetColor();                      // Obnov puvodni barvu textu
                        break;
                }

                // Pokud program stale bezi, cekame na Enter pred dalsim zobrazenim menu
                if (bezi)
                {
                    Console.WriteLine("\nStisknete Enter pro pokracovani...");
                    Console.ReadLine(); // Cekame na stisk Enter (vstup nezpracovavame)
                }
            }
        }

        // ============================================================
        // METODA ZobrazitMenu
        // Vykresli hlavni menu v konzoli
        // ============================================================
        static void ZobrazitMenu()
        {
            Console.Clear(); // Vymaze obsah konzole

            // Nastavime modrou barvu pro nadpis
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("==========================================");
            Console.WriteLine("    TETOVACI STUDIO - Sprava objednavek   ");
            Console.WriteLine("==========================================");
            Console.ResetColor(); // Vratime puvodni barvu

            // Vypis polozek menu
            Console.WriteLine();
            Console.WriteLine("  1 - Zobrazit vsechny objednavky");
            Console.WriteLine("  2 - Pridat novou objednavku");
            Console.WriteLine("  3 - Odstranit objednavku");
            Console.WriteLine("  4 - Vyhledat klienta");
            Console.WriteLine("  5 - Statistiky (prijmy)");
            Console.WriteLine("  6 - Oznacit objednavku jako zaplacenou");
            Console.WriteLine("  0 - Ulozit a ukoncit");
            Console.WriteLine();
            Console.Write("Vase volba: "); // Write (bez ln) = bez odradkovani, kurzor zustane na stejnem radku
        }

        // ============================================================
        // METODA ZobrazitVsechny
        // Vypise vsechny objednavky jako prehlednou tabulku
        // ============================================================
        static void ZobrazitVsechny()
        {
            Console.Clear(); // Vymaze konzoli

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== SEZNAM VSECH OBJEDNAVEK ===\n");
            Console.ResetColor();

            // Overime jestli vubec nejake objednavky existuji
            if (objednavky.Count == 0)
            {
                Console.WriteLine("Zadne objednavky nenalezeny."); // Seznam je prazdny
                return; // Ukoncime metodu, dal nepokracujeme
            }

            // Hlavicka tabulky
            // ,-4 znamena: zabere minimalne 4 znaky, zarovnano doleva - tak jsou sloupce rovne
            Console.WriteLine($"{"ID",-4} {"Klient",-20} {"Telefon",-14} {"Datum",-12} {"Motiv",-20} {"Cena",-10} {"Zapl."}");
            Console.WriteLine(new string('-', 90)); // Oddelovaci cara - 90x pomlcka za sebou

            // Projdeme kazdu objednavku v seznamu (foreach = cyklus pres vsechny prvky)
            foreach (var objednavka in objednavky)
            {
                // Ternary operator: podminka ? kdyz_ano : kdyz_ne
                // Pokud je zaplaceno = true → "ANO", jinak → "NE"
                string zaplaceno = objednavka.Zaplaceno ? "ANO" : "NE";

                // Zaplacene objednavky zobrazime zelenou barvou, nezaplacene bilou
                Console.ForegroundColor = objednavka.Zaplaceno ? ConsoleColor.Green : ConsoleColor.White;

                // Vypiseme radek tabulky s daty objednavky
                // :dd.MM.yyyy = format data,  :F0 = cislo bez desetinnych mist
                Console.WriteLine($"{objednavka.Id,-4} {objednavka.JmenoKlienta,-20} {objednavka.Telefon,-14} {objednavka.DatumNavstevy.ToString("dd.MM.yyyy"),-12} {objednavka.PopisTetovani,-20} {objednavka.Cena,-10:F0} {zaplaceno}");

                Console.ResetColor(); // Po kazdem radku vratime puvodni barvu
            }

            // Celkovy pocet objednavek
            Console.WriteLine($"\nCelkem objednavek: {objednavky.Count}");
        }

        // ============================================================
        // METODA PridatObjednavku
        // Interaktivne prida novou objednavku - postupne se pta na vsechny udaje
        // Kazdy vstup je overeny - program neprijme spatne zadane udaje
        // ============================================================
        static void PridatObjednavku()
        {
            Console.Clear(); // Vymaze konzoli

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== PRIDAT NOVOU OBJEDNAVKU ===\n");
            Console.ResetColor();

            // Vytvorime novy prazdny objekt objednavky (jako prazdny formular)
            var nova = new Objednavka();

            // Priradime nasledujici volne ID a zvysime pocitadlo o 1
            // dalsiId++ = pouzij aktualni hodnotu, pak ji zvys o 1
            nova.Id = dalsiId++;

            // -------------------------------------------------------
            // OVERENY VSTUP 1: Jmeno klienta
            // Podminky: nesmi byt prazdne a musi mit alespon 3 znaky
            // while(true) = nekonecna smycka, vyskocime z ni jedine pres break
            // -------------------------------------------------------
            while (true)
            {
                Console.Write("Jmeno a prijmeni klienta: "); // Vypiseme otazku
                string jmeno = Console.ReadLine()?.Trim();   // Precteme vstup, odstranime mezery na krajich

                // IsNullOrEmpty = overi jestli retezec neni prazdny nebo null (nic)
                if (!string.IsNullOrEmpty(jmeno) && jmeno.Length >= 3) // Pokud je jmeno v poradku
                {
                    nova.JmenoKlienta = jmeno; // Ulozime jmeno do objednavky
                    break;                     // Vyskocime ze smycky, jdeme dal
                }

                // Pokud jmeno nevyhovuje - vypiseme chybu a smycka se opakuje
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Chyba: Jmeno musi mit alespon 3 znaky!");
                Console.ResetColor();
            }

            // -------------------------------------------------------
            // OVERENY VSTUP 2: Telefonni cislo
            // Podminky: presne 9 znaku a vsechny musi byt cislice
            // -------------------------------------------------------
            while (true)
            {
                Console.Write("Telefonni cislo (9 cislic): ");
                string tel = Console.ReadLine()?.Trim().Replace(" ", ""); // Odstranime pripadne mezery

                // Overime: neni prazdne + presne 9 znaku + vsechny jsou cislice
                if (!string.IsNullOrEmpty(tel) && tel.Length == 9 && IsVsechnyCislice(tel))
                {
                    nova.Telefon = tel; // Ulozime telefon
                    break;             // Vyskocime ze smycky
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Chyba: Telefon musi obsahovat presne 9 cislic!");
                Console.ResetColor();
            }

            // -------------------------------------------------------
            // OVERENY VSTUP 3: Datum navstevy
            // Podminky: format dd.MM.yyyy a datum nesmi byt v minulosti
            // -------------------------------------------------------
            while (true)
            {
                Console.Write("Datum navstevy (dd.MM.yyyy): ");
                string datumStr = Console.ReadLine()?.Trim();

                // TryParseExact = zkusi prevest text na datum
                // Pokud se to povede → ulozi datum do promenne "datum" a vrati true
                // Pokud se to nepovede → vrati false (program NESPADNE s chybou)
                if (DateTime.TryParseExact(datumStr, "dd.MM.yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime datum))
                {
                    // Datum je ve spravnem formatu, ted overime jestli neni v minulosti
                    if (datum.Date >= DateTime.Today) // DateTime.Today = dnesni datum
                    {
                        nova.DatumNavstevy = datum; // Ulozime datum
                        break;                      // Vyskocime ze smycky
                    }
                    // Datum je v minulosti
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Chyba: Datum musi byt dnesni nebo budouci!");
                    Console.ResetColor();
                }
                else
                {
                    // Spatny format data
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Chyba: Neplatny format data! Pouzijte dd.MM.yyyy");
                    Console.ResetColor();
                }
            }

            // -------------------------------------------------------
            // Popis tetovani - jedina podminka je ze nesmi byt prazdny
            // -------------------------------------------------------
            while (true)
            {
                Console.Write("Popis motivu tetovani: ");
                string popis = Console.ReadLine()?.Trim();

                if (!string.IsNullOrEmpty(popis)) // Pokud neni prazdny
                {
                    nova.PopisTetovani = popis; // Ulozime popis
                    break;                      // Vyskocime
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Chyba: Popis nesmi byt prazdny!");
                Console.ResetColor();
            }

            // -------------------------------------------------------
            // OVERENY VSTUP 4: Cena
            // Podminky: musi to byt cislo v rozsahu 500 az 100000
            // -------------------------------------------------------
            while (true)
            {
                Console.Write("Cena (Kc, 500 - 100000): ");

                // Replace(",", ".") = nahrad carku teckou, kdyby uzivatel napsal 2500,50
                string cenaStr = Console.ReadLine()?.Trim().Replace(",", ".");

                // TryParse = zkusi prevest text na desetinne cislo (nespadne pri chybe)
                if (double.TryParse(cenaStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double cena)
                    && cena >= 500      // Minimalni cena
                    && cena <= 100000)  // Maximalni cena
                {
                    nova.Cena = cena; // Ulozime cenu
                    break;           // Vyskocime
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Chyba: Cena musi byt cislo mezi 500 a 100000!");
                Console.ResetColor();
            }

            // Nova objednavka je vychozi nezaplacena
            nova.Zaplaceno = false;

            // Pridame objednavku do seznamu v pameti
            objednavky.Add(nova);

            // Hned ulozime do souboru aby se data neztratila
            UlozitData();

            // Vypiseme zpravu o uspechu
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nObjednavka #{nova.Id} pro klienta {nova.JmenoKlienta} byla pridana!");
            Console.ResetColor();
        }

        // ============================================================
        // METODA OdstranitObjednavku
        // Smaze vybranou objednavku podle jejiho ID cisla
        // ============================================================
        static void OdstranitObjednavku()
        {
            Console.Clear(); // Vymaze konzoli

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== ODSTRANIT OBJEDNAVKU ===\n");
            Console.ResetColor();

            // Nejdrive zobrazime vsechny objednavky aby uzivatel videl jejich ID
            ZobrazitVsechny();

            // Pokud je seznam prazdny, neni co mazat
            if (objednavky.Count == 0)
            {
                return; // Ukoncime metodu
            }

            Console.Write("\nZadejte ID objednavky k odstraneni (0 = zrusit): ");
            string vstup = Console.ReadLine()?.Trim(); // Precteme vstup

            // Pokud uzivatel zadal 0, akci zrusime
            if (vstup == "0")
            {
                return; // Odejdeme bez smazani
            }

            // TryParse = zkusi prevest text na cele cislo
            // Pokud se to povede, cislo se ulozi do promenne "id"
            if (int.TryParse(vstup, out int id))
            {
                // Find() = prohledá seznam a vrati prvni objednavku ktera splnuje podmínku
                // o => o.Id == id = pro kazdy prvek "o" zkontroluj jestli jeho Id odpovida hledanemu
                var nalezena = objednavky.Find(o => o.Id == id);

                // Overime jestli jsme neco nasli
                if (nalezena != null) // null = nic nenalezeno
                {
                    // Pozadame potvrzeni pred smazanim
                    Console.Write($"Opravdu smazat objednavku #{id} ({nalezena.JmenoKlienta})? (a/n): ");
                    string potvrzeni = Console.ReadLine()?.Trim().ToLower(); // ToLower() = prevod na mala pismena

                    if (potvrzeni == "a" || potvrzeni == "ano") // Pokud uzivatel potvrdil
                    {
                        objednavky.Remove(nalezena); // Smazeme ze seznamu v pameti
                        UlozitData();               // Ulozime aktualizovany seznam do souboru

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Objednavka byla odstranena.");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine("Akce zrusena."); // Uzivatel smazani odmitnul
                    }
                }
                else // Objednavka s timto ID neexistuje
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Objednavka s ID {id} nebyla nalezena!");
                    Console.ResetColor();
                }
            }
            else // Uzivatel nezadal cislo
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Neplatne ID! Zadejte cislo.");
                Console.ResetColor();
            }
        }

        // ============================================================
        // METODA VyhledatKlienta
        // Vyhleda objednavky podle jmena klienta
        // Vyhledavani nerozeznava velka a mala pismena (Jan = jan = JAN)
        // ============================================================
        static void VyhledatKlienta()
        {
            Console.Clear(); // Vymaze konzoli

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== VYHLEDANI KLIENTA ===\n");
            Console.ResetColor();

            Console.Write("Zadejte jmeno nebo cast jmena: ");
            string hledani = Console.ReadLine()?.Trim().ToLower(); // Prevedeme na mala pismena pro porovnani

            // Overime ze uzivatel neco zadal
            if (string.IsNullOrEmpty(hledani))
            {
                Console.WriteLine("Hledany vyraz nesmi byt prazdny.");
                return; // Odejdeme
            }

            // FindAll() = najde VSECHNY prvky ktere splnuji podmínku (ne jen prvni)
            // Contains() = overi jestli retezec obsahuje hledany vyraz
            var vysledky = objednavky.FindAll(o => o.JmenoKlienta.ToLower().Contains(hledani));

            // Pokud nic nenasli
            if (vysledky.Count == 0)
            {
                Console.WriteLine("Zadny klient nenalezen.");
                return;
            }

            // Vypiseme nalezene vysledky
            Console.WriteLine($"\nNalezeno {vysledky.Count} vysledek(ku):\n");
            Console.WriteLine($"{"ID",-4} {"Klient",-20} {"Telefon",-14} {"Datum",-12} {"Motiv",-20} {"Cena",-10} {"Zapl."}");
            Console.WriteLine(new string('-', 90)); // Oddelovaci cara

            // Projdeme vsechny nalezene vysledky
            foreach (var o in vysledky)
            {
                string zaplaceno = o.Zaplaceno ? "ANO" : "NE"; // Stav platby
                Console.WriteLine($"{o.Id,-4} {o.JmenoKlienta,-20} {o.Telefon,-14} {o.DatumNavstevy.ToString("dd.MM.yyyy"),-12} {o.PopisTetovani,-20} {o.Cena,-10:F0} {zaplaceno}");
            }
        }

        // ============================================================
        // METODA ZobrazitStatistiku
        // Spocita a zobrazi financni prehled studia
        // ============================================================
        static void ZobrazitStatistiku()
        {
            Console.Clear(); // Vymaze konzoli

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== STATISTIKY STUDIA ===\n");
            Console.ResetColor();

            // Pokud nejsou zadna data, neni co pocitat
            if (objednavky.Count == 0)
            {
                Console.WriteLine("Zadna data k zobrazeni.");
                return;
            }

            // Promenne pro scitani
            double celkemCena = 0; // Celkova castka vsech objednavek
            double zaplacenoCena = 0; // Castka pouze zaplatených objednavek
            int pocetZaplaceno = 0; // Pocet zaplatených
            int pocetNezaplaceno = 0; // Pocet nezaplatených

            // Projdeme vsechny objednavky a scitame
            foreach (var o in objednavky)
            {
                celkemCena += o.Cena; // Pricteme cenu k celkove sume  (+= znamena: celkemCena = celkemCena + o.Cena)

                if (o.Zaplaceno) // Pokud je zaplaceno
                {
                    zaplacenoCena += o.Cena; // Pricteme do zaplacených
                    pocetZaplaceno++;         // Zvysime pocitadlo (++ = +1)
                }
                else // Pokud neni zaplaceno
                {
                    pocetNezaplaceno++; // Zvysime pocitadlo nezaplacených
                }
            }

            // Dluh = celkova suma minus zaplacena cast
            double nezaplacenoCena = celkemCena - zaplacenoCena;

            // Vypiseme vysledky
            Console.WriteLine($"Celkem objednavek:      {objednavky.Count}"); // Celkovy pocet
            Console.WriteLine($"Zaplaceno:              {pocetZaplaceno}");    // Pocet zaplacených
            Console.WriteLine($"Nezaplaceno:            {pocetNezaplaceno}");  // Pocet dlužníků
            Console.WriteLine(); // Prazdny radek pro oddelenI

            Console.ForegroundColor = ConsoleColor.Green; // Zelena pro kladne cisla
            Console.WriteLine($"Celkove prijmy (vse):   {celkemCena:F0} Kc");    // :F0 = bez desetinnych mist
            Console.WriteLine($"Prijate platby:         {zaplacenoCena:F0} Kc"); // Skutecne prijate penize
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Red; // Cervena pro dluhy
            Console.WriteLine($"Pohledavky (dluhy):     {nezaplacenoCena:F0} Kc"); // Nezaplacene castky
            Console.ResetColor();

            // Najdeme nejdrazsi objednavku
            // Zacneme tim ze za "zatim nejdrazsi" povazujeme prvni objednavku v seznamu
            var nejdrazsi = objednavky[0]; // [0] = prvni prvek seznamu

            foreach (var o in objednavky) // Projdeme vsechny
            {
                if (o.Cena > nejdrazsi.Cena) // Pokud je aktualni drazsi nez "zatim nejdrazsi"
                {
                    nejdrazsi = o; // Aktualizujeme "nejdrazsi"
                }
            }

            Console.WriteLine(); // Prazdny radek
            Console.WriteLine($"Nejdrazsi objednavka:   #{nejdrazsi.Id} - {nejdrazsi.JmenoKlienta} ({nejdrazsi.Cena:F0} Kc)");
        }

        // ============================================================
        // METODA OznacitZaplaceno
        // Oznaci vybranou objednavku jako zaplacenou
        // ============================================================
        static void OznacitZaplaceno()
        {
            Console.Clear(); // Vymaze konzoli

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== OZNACIT JAKO ZAPLACENO ===\n");
            Console.ResetColor();

            // Vyfiltrujeme pouze nezaplacene objednavky
            // !o.Zaplaceno = Zaplaceno je false (tedy nezaplaceno)
            var nezaplacene = objednavky.FindAll(o => !o.Zaplaceno);

            // Pokud jsou vsechny uz zaplaceny
            if (nezaplacene.Count == 0)
            {
                Console.WriteLine("Vsechny objednavky jsou jiz zaplaceny!");
                return;
            }

            // Zobrazime pouze nezaplacene
            Console.WriteLine("Nezaplacene objednavky:");
            Console.WriteLine($"{"ID",-4} {"Klient",-20} {"Datum",-12} {"Cena"}");
            Console.WriteLine(new string('-', 50)); // Oddelovaci cara

            foreach (var o in nezaplacene) // Vypiseme kazdou nezaplacenou
            {
                Console.WriteLine($"{o.Id,-4} {o.JmenoKlienta,-20} {o.DatumNavstevy.ToString("dd.MM.yyyy"),-12} {o.Cena:F0} Kc");
            }

            Console.Write("\nZadejte ID objednavky (0 = zrusit): ");
            string vstup = Console.ReadLine()?.Trim(); // Precteme ID

            if (vstup == "0") return; // Zrusime pokud uzivatel zadal 0

            if (int.TryParse(vstup, out int id)) // Pokud bylo zadano cislo
            {
                // Hledame objednavku ktera ma dane ID a jeste neni zaplacena
                var nalezena = objednavky.Find(o => o.Id == id && !o.Zaplaceno);

                if (nalezena != null) // Pokud jsme nasli
                {
                    nalezena.Zaplaceno = true; // Oznacime jako zaplacenou
                    UlozitData();             // Ulozime zmenu do souboru

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Objednavka #{id} oznacena jako zaplacena. Prijato: {nalezena.Cena:F0} Kc");
                    Console.ResetColor();
                }
                else // Objednavka nenalezena nebo uz zaplacena
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Objednavka nenalezena nebo je jiz zaplacena.");
                    Console.ResetColor();
                }
            }
        }

        // ============================================================
        // METODA NacistData
        // Nacte objednavky z CSV souboru pri spusteni programu
        // DULEZITE: program NESPADNE pri chybe - vse je v try-catch
        // ============================================================
        static void NacistData()
        {
            // Overime jestli soubor vubec existuje na disku
            if (!File.Exists(csouboru)) // File.Exists() = true pokud soubor existuje
            {
                // Soubor neexistuje = prvni spusteni programu, nic se nedeje
                Console.WriteLine("Soubor s daty nenalezen. Zaciname s prazdnym seznamem.");
                return; // Odejdeme z metody
            }

            // try-catch = bezpecne provedeni kodu
            // Pokud v bloku "try" nastane chyba → spusti se blok "catch"
            // Program NESPADNE, ale pokracuje dal
            try
            {
                // Precteme VSECHNY radky souboru najednou do pole retezcu
                string[] radky = File.ReadAllLines(csouboru, System.Text.Encoding.UTF8);

                int nacteno = 0; // Pocitadlo uspesne nactenych objednavek
                int chybne = 0; // Pocitadlo poskozenych radku

                // Projdeme kazdy radek souboru
                foreach (string radek in radky)
                {
                    // Preskocime prazdne radky
                    if (string.IsNullOrWhiteSpace(radek))
                        continue; // continue = preskoc tento krok a jdi na dalsi radek

                    // Preskocime hlavickovy radek (prvni radek souboru zacina "ID")
                    if (radek.StartsWith("ID"))
                        continue; // StartsWith() = true pokud retezec zacina danym textem

                    // Vnitrni try-catch pro kazdy radek zvlast
                    // Pokud je jeden radek poskozeny, preskocime jen ten - ne cely soubor
                    try
                    {
                        // Zavolame metodu ktera prevede radek textu na objekt objednavky
                        Objednavka o = Objednavka.ZCsvRadku(radek);
                        objednavky.Add(o); // Pridame objednavku do seznamu v pameti

                        // Aktualizujeme pocitadlo ID aby nova ID navazovala na posledni
                        if (o.Id >= dalsiId)
                            dalsiId = o.Id + 1; // Dalsi ID = posledni + 1

                        nacteno++; // Zvysime pocet uspesne nactenych
                    }
                    catch (Exception) // Pokud je radek poskozeny
                    {
                        chybne++; // Pocitame poskozene radky
                        // Pokracujeme dal, program nespadne
                    }
                }

                // Vypiseme vysledek nacteni
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Nacteno {nacteno} objednavek ze souboru.");
                Console.ResetColor();

                // Pokud byly nejake poskozene radky, upozornime uzivatele
                if (chybne > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Varovani: {chybne} poskozeny radek(ku) byl preskocen.");
                    Console.ResetColor();
                }
            }
            catch (Exception ex) // Pokud se soubor nepodarilo vubec otevrit
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Chyba pri nacteni: {ex.Message}"); // ex.Message = popis chyby
                Console.WriteLine("Pokracujeme s prazdnym seznamem.");
                Console.ResetColor();
            }

            // Kratkka pauza aby uzivatel stihl precist zpravu pred zobrazenim menu
            System.Threading.Thread.Sleep(1000); // 1000 = 1 sekunda
        }

        // ============================================================
        // METODA UlozitData
        // Ulozi vsechny objednavky do CSV souboru
        // ============================================================
        static void UlozitData()
        {
            try // Zkusime ulozit
            {
                // StreamWriter = trida pro zapis textu do souboru
                // false = prepis soubor celý (nepridavej na konec)
                // Encoding.UTF8 = kodovani pro podporu ceskych znaku
                using (StreamWriter sw = new StreamWriter(csouboru, false, System.Text.Encoding.UTF8))
                {
                    // Zapiseme hlavickovy radek aby byl soubor srozumitelny
                    sw.WriteLine("ID;JmenoKlienta;Telefon;DatumNavstevy;PopisTetovani;Cena;Zaplaceno");

                    // Zapiseme kazdou objednavku jako jeden radek
                    foreach (var o in objednavky)
                    {
                        sw.WriteLine(o.ToCsvRadek()); // Zavolame metodu ktera prevede objekt na CSV radek
                    }
                } // "using" automaticky zavre soubor kdyz se dostaneme sem
            }
            catch (Exception ex) // Pokud se ulozeni nepodarilo
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Chyba pri ukladani dat: {ex.Message}"); // Vypiseme popis chyby
                Console.ResetColor();
            }
        }

        // ============================================================
        // POMOCNA METODA IsVsechnyCislice
        // Overi jestli retezec obsahuje pouze cislice (0-9)
        // Vraci true pokud ANO, false pokud obsahuje neco jineho
        // ============================================================
        static bool IsVsechnyCislice(string text)
        {
            // Projdeme kazdy znak retezce jeden po druhem
            foreach (char c in text)
            {
                // IsDigit() = true pokud je znak cislice (0-9)
                if (!char.IsDigit(c)) // Pokud znak NENI cislice
                {
                    return false; // Vratime false - nasli jsme necislicny znak
                }
            }
            return true; // Vsechny znaky jsou cislice - vratime true
        }
    }
}
