using Microsoft.AspNetCore.Components;
using SnapshotDiff.Features.Help;

namespace SnapshotDiff.Features.Help.UI;

public partial class HelpPage
{
    private static readonly List<HelpSection> _sections =
    [
        new()
        {
            Id = "overview",
            Title = "Overview",
            Body = """
                <p>
                    <strong>SnapshotDiff</strong> je čistič souborů. Naskenuje vybraný adresář,
                    umožní filtrovat soubory podle stáří, velikosti nebo přípony a přesune
                    nebo trvale odstraní vybrané soubory.
                </p>
                <p class="mt-2">
                    Typické použití:
                </p>
                <ul class="list-disc ml-5 mt-1 space-y-1">
                    <li>Nalezení starých nebo zbytečných souborů v libovolném adresáři</li>
                    <li>Čištění dočasných souborů, logů a zbytků po odinstalaci aplikací</li>
                    <li>Bezpečné přesení souborů do koše před trvalým odstraněním</li>
                    <li>Export seznamu souborů do JSON nebo CSV pro další analýzu</li>
                </ul>
                """
        },
        new()
        {
            Id = "watching",
            Title = "Watched Directories",
            Body = """
                <p>
                    Přejděte na stránku <strong>Nastavení → Sledované adresáře</strong> a přidejte složky,
                    které chcete skenovat.
                </p>
                <ul class="list-disc ml-5 mt-2 space-y-1">
                    <li>Klikněte <em>Přidat adresář</em> a zadejte cestu (např. <code class="code">C:\Users\Vy\Downloads</code>)</li>
                    <li>Volitelně pojmenujte adresář pro snadnější identifikaci</li>
                    <li>Pomocí přepínače dočasně zakažte skenování bez odebrání adresáře</li>
                    <li>Každý adresář může mít vlastní nastavení filtrů</li>
                </ul>
                <p class="mt-2">
                    Vhodné adresáře k přidání:
                </p>
                <ul class="list-disc ml-5 mt-1 space-y-1">
                    <li><code class="code">%Temp%</code> — dočasné soubory systému a aplikací</li>
                    <li><code class="code">%LocalAppData%</code> — lokální cache aplikací</li>
                    <li><code class="code">%AppData%\Roaming</code> — uživatelská data aplikací</li>
                    <li><code class="code">%Downloads%</code> — složka stažených souborů</li>
                </ul>
                """
        },
        new()
        {
            Id = "scanning",
            Title = "Scanning",
            Body = """
                <p>
                    Na stránce <strong>Skenovat</strong> vyberte adresář a klikněte <em>Spustit skenování</em>.
                    Průběhový pruh zobrazuje aktuálně zpracovávaný adresář a skenování lze kdykoli zrušit.
                </p>
                <p class="mt-2">
                    Po dokončení skenování se výsledky zobrazí v seznamu souborů pod průběhovým pruhem.
                </p>
                <ul class="list-disc ml-5 mt-2 space-y-1">
                    <li>Skenování prochází celý strom adresářů rekurzivně</li>
                    <li>Výsledky jsou uloženy v paměti — pro aktuální stav vždy spusťte nové skenování</li>
                    <li>Soubory vyloučené pravidly vyloučení se v seznamu neobjeví</li>
                    <li>Skenování neprovádí hašování souborů ani nevytváří snímky</li>
                </ul>
                """
        },
        new()
        {
            Id = "filters",
            Title = "Filters",
            Body = """
                <p>
                    Filtry umožňují zobrazit pouze soubory splňující zadaná kritéria.
                    Jsou aplikovány v paměti — odezva je okamžitá bez nutnosti nového skenování.
                </p>
                <ul class="list-disc ml-5 mt-2 space-y-1">
                    <li><strong>Staré soubory</strong> — <code class="code">LastWriteTime</code> starší než X dní</li>
                    <li><strong>Nové soubory</strong> — <code class="code">LastWriteTime</code> v posledních X dnech</li>
                    <li><strong>Přípona</strong> — zadejte čárkou oddělené přípony, např. <code class="code">.log, .tmp</code></li>
                    <li><strong>Minimální velikost</strong> — filtruje soubory menší než zadaná velikost</li>
                    <li><strong>Hledání podle jména</strong> — fulltextové hledání v názvu souboru (bez rozlišení velikosti písmen)</li>
                </ul>
                <p class="mt-2">
                    Kombinace více filtrů funguje jako logické AND — zobrazí se pouze soubory splňující všechna kritéria.
                </p>
                """
        },
        new()
        {
            Id = "filelist",
            Title = "File List & Selection",
            Body = """
                <p>
                    Seznam souborů zobrazuje výsledky posledního skenování s aplikovanými filtry.
                </p>
                <ul class="list-disc ml-5 mt-2 space-y-1">
                    <li>Každý soubor má zaškrtávací políčko pro výběr</li>
                    <li>Použijte <em>Vybrat vše</em> pro výběr všech zobrazených souborů</li>
                    <li>Kliknutím na záhlaví sloupce změníte řazení (jméno, velikost, datum, přípona)</li>
                    <li>Výběrem řádku složky vyberete všechny soubory v dané složce</li>
                </ul>
                <p class="mt-2">
                    Po výběru souborů použijte tlačítka <em>Přesunout do koše</em> nebo <em>Trvale smazat</em>.
                </p>
                """
        },
        new()
        {
            Id = "trash",
            Title = "Trash",
            Body = """
                <p>
                    Koš umožňuje bezpečně přesunout soubory před jejich trvalým odstraněním.
                </p>
                <ul class="list-disc ml-5 mt-2 space-y-1">
                    <li>Klikněte <em>Přesunout do koše</em> v seznamu souborů pro přesun vybraných souborů</li>
                    <li>Soubory v koši jsou uchovávány po dobu <strong>30 dní</strong></li>
                    <li>Na stránce <strong>Koš</strong> lze soubory obnovit do původního umístění</li>
                    <li>Soubory lze trvale smazat jednotlivě nebo vyprázdnit celý koš najednou</li>
                    <li>Po uplynutí 30 dní jsou soubory automaticky odstraněny</li>
                </ul>
                <p class="mt-2">
                    <strong>Upozornění:</strong> obnovení souboru selže, pokud původní umístění již neexistuje.
                </p>
                """
        },
        new()
        {
            Id = "exclusions",
            Title = "Exclusion Rules",
            Body = """
                <p>
                    Pravidla vyloučení zabraňují zobrazení určitých souborů a adresářů ve výsledcích skenování.
                </p>
                <h4 class="font-medium mt-3 mb-1" style="color: var(--text)">Systémová pravidla</h4>
                <p>
                    Vestavěná pravidla chrání důležité systémové soubory operačního systému.
                    Jsou pouze pro čtení a nelze je upravovat.
                </p>
                <h4 class="font-medium mt-3 mb-1" style="color: var(--text)">Uživatelská pravidla</h4>
                <p>
                    Na stránce <strong>Vyloučení</strong> lze přidat vlastní vzory ve formátu glob:
                </p>
                <ul class="list-disc ml-5 mt-1 space-y-1">
                    <li><code class="code">node_modules</code> — přesná shoda jména složky</li>
                    <li><code class="code">*.tmp</code> — všechny soubory s příponou .tmp</li>
                    <li><code class="code">*.log</code> — všechny soubory s příponou .log</li>
                    <li><code class="code">C:\specifická\cesta</code> — absolutní cesta jako prefix</li>
                </ul>
                """
        },
        new()
        {
            Id = "export",
            Title = "Export",
            Body = """
                <p>
                    Ze stránky <strong>Skenovat</strong> použijte tlačítko <em>Exportovat</em> pro uložení
                    aktuálního seznamu souborů.
                </p>
                <ul class="list-disc ml-5 mt-2 space-y-1">
                    <li><strong>JSON</strong> — zachovává veškerá metadata; vhodné pro strojové zpracování</li>
                    <li><strong>CSV</strong> — tabulkový formát, snadno otevřitelný v Excelu nebo skriptech</li>
                </ul>
                <p class="mt-2">
                    Exportovaný soubor obsahuje pro každý soubor: úplnou cestu, jméno, příponu,
                    velikost, datum poslední změny a typ záznamu.
                </p>
                """
        },
        new()
        {
            Id = "tips",
            Title = "Tips & Recommendations",
            Body = """
                <h4 class="font-medium mb-1" style="color: var(--text)">Vhodné adresáře ke skenování</h4>
                <ul class="list-disc ml-5 mt-1 space-y-1">
                    <li><code class="code">%Temp%</code> — nejrychlejší zdroj uvolnitelného místa</li>
                    <li><code class="code">%LocalAppData%\Temp</code> — dočasné soubory aplikací</li>
                    <li><code class="code">%Downloads%</code> — stažené soubory, které již nejsou potřeba</li>
                    <li><code class="code">%LocalAppData%</code> — caches (může být velký, skenujte opatrně)</li>
                </ul>
                <h4 class="font-medium mt-3 mb-1" style="color: var(--text)">Tipy pro výkon</h4>
                <ul class="list-disc ml-5 mt-1 space-y-1">
                    <li>Přidejte pravidla vyloučení pro velké adresáře jako <code class="code">node_modules</code></li>
                    <li>Při prvním skenování velkého adresáře počítejte s delší dobou čekání</li>
                    <li>Filtry jsou aplikovány v paměti — přidejte je před skenováním pro lepší přehlednost</li>
                </ul>
                <h4 class="font-medium mt-3 mb-1" style="color: var(--text)">Umístění dat</h4>
                <p>
                    Konfigurace a databáze koše jsou uloženy v
                    <code class="code">%AppData%\SnapshotDiff\</code>.
                    Tuto složku lze zobrazit v <strong>Nastavení → Úložiště dat</strong>.
                </p>
                """
        },
    ];
}
