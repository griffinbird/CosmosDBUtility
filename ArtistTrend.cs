using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CosmosDemo
{
    public enum TrendDirection
    {
        Increasing, Decreasing, Stable, Bouncing
    }

    public class ArtistTrend
    {
        [JsonProperty]
        private int activeUsersListening;

        [JsonConverter(typeof(StringEnumConverter))]
        public TrendDirection currentDirection;

        [JsonProperty(PropertyName = "id")]
        public string artistListenCountId;

        public string artistName;

        public string internalName;

        public string trendingCountry;

        private int minimumListens = 0;
        private int maximumListens = 1000000;

        private static Random rnd = new Random();

        [JsonProperty]
        private DateTime artistListenCountTimestamp;

        public ArtistTrend() { }

        public ArtistTrend(int activeUsersListening, string internalName)
        {
            this.currentDirection = TrendDirection.Stable;
            this.activeUsersListening = activeUsersListening;
            this.internalName = internalName;
            this.artistName = pickRandomArtist();
        }

        public ArtistTrend(string internalName) : this(internalName, "Unknown")
        {

        }

        public ArtistTrend(string internalName, string trendingCountry)
        {
            this.trendingCountry = trendingCountry;
            this.currentDirection = TrendDirection.Stable;           
            this.activeUsersListening = rnd.Next(100, 500); ;
            this.internalName = internalName;
            this.artistName = pickRandomArtist();
        }

        private string pickRandomArtist()
        {
            return artistList[rnd.Next(0,artistList.Length-1)];
        }

        public string ReadAsJSON()
        {
            this.ReadActiveUsers(false);
            return JsonConvert.SerializeObject(this);
        }

        public string ReadAsJSONConflict()
        {
            this.ReadActiveUsers(true);
            return JsonConvert.SerializeObject(this);
        }

        public override string ToString()
        {
            return this.ReadAsJSON();
        }

        public int ReadActiveUsers(bool conflict)
        {
            int delta = (int)rnd.Next(10, 50);
            int sign = 1;

            if ((this.activeUsersListening <= minimumListens) &&
                ((this.currentDirection == TrendDirection.Decreasing) || (this.currentDirection == TrendDirection.Bouncing))
                ||
                    ((this.activeUsersListening >= maximumListens) &&
                ((this.currentDirection == TrendDirection.Increasing) || (this.currentDirection == TrendDirection.Bouncing))))
                this.currentDirection = TrendDirection.Stable;

            switch (this.currentDirection)
            {
                case TrendDirection.Bouncing:
                    sign = (rnd.Next(2) == 0) ? 1 : -1;
                    break;
                case TrendDirection.Increasing:
                    sign = 1;
                    break;
                case TrendDirection.Decreasing:
                    sign = -1;
                    break;
                case TrendDirection.Stable:
                    delta = 0;
                    break;
            }
            this.activeUsersListening = activeUsersListening + (delta * sign);
            var rightNow = DateTime.UtcNow;
            if (!conflict)
                this.artistListenCountTimestamp = rightNow;
            else
                this.artistListenCountTimestamp = new DateTime(rightNow.Year, rightNow.Month, rightNow.Day);

            this.artistListenCountId = this.internalName + "_" + this.trendingCountry + "_" + this.artistListenCountTimestamp.ToString("yyyyMMddHH:mm:ss.ffff");
                return this.activeUsersListening;
        }

        public static string[] artistList  = {
         "greenday"
,
"slipknot"
,
"311"
,
"3 doors down"
,
"50 cent"
,
"adele"
,
"alanis morrisette"
,
"alicia keys"
,
"arcade fire"
,
"avenged sevenfold"
,
"avicii"
,
"axwell"
,
"bassnectar"
,
"ben harper"
,
"big & rich"
,
"black eyed peas"
,
"blake shelton"
,
"bob dylan"
,
"bon jovi"
,
"bonnie raitt"
,
"brantley gilbert"
,
"breaking benjamin"
,
"britney spears"
,
"bruce springsteen"
,
"bruno mars"
,
"caifanes"
,
"calvin harris"
,
"carrie underwood"
,
"cee lo green"
,
"chris brown"
,
"christina aguilera"
,
"coldplay"
,
"counting crows"
,
"creed"
,
"damian marley"
,
"daughtry"
,
"dave matthews"
,
"david gray"
,
"david guetta"
,
"deadmau5"
,
"demi lovato"
,
"dierks bentley"
,
"dj red foo"
,
"drake"
,
"ed shereen"
,
"ellie goulding"
,
"eric church"
,
"faith hill"
,
"fall out boy"
,
"fantasia"
,
"flo rida"
,
"jessica simpson"
,
"john legend"
,
"john mayer"
,
"josh groban"
,
"juanes"
,
"justin bieber"
,
"justin timberlake"
,
"kanye west"
,
"kaskade"
,
"katherine mcphee"
,
"katy perry"
,
"ke$ha"
,
"keith urban"
,
"kelly clarkson"
,
"kendrick lamar"
,
"kid cudi"
,
"kid rock"
,
"kings of leon"
,
"kip moore"
,
"lady antebellum"
,
"lady gaga"
,
"lil' wayne"
,
"linkin park"
,
"little big town"
,
"ll cool j"
,
"lmfao"
,
"lorde"
,
"luke bryan"
,
"macklemore"
,
"madonna"
,
"maroon 5"
,
"martina mcbride"
,
"matchbox 20"
,
"maxwell"
,
"merle haggard"
,
"miranda lambert"
,
"morrissey"
,
"mumford & sons"
,
"muse"
,
"myth busters"
,
"nicki minaj"
,
"nickleback"
,
"no doubt"
,
"of monsters & men"
,
"one direction"
,
"one republic"
,
"paramore"
,
"passion pit"
,
"pearl jam"
,
"pharrell williams"
,
"phoenix"
,
"pitbull"
,
"pretty lights"
,
"psy"
,
"sublime w/ rome"
,
"sugarland"
,
"susan boyle"
,
"taylor swift"
,
"the avett bros"
,
"the band perry"
,
"the flaming lips"
,
"the fray"
,
"the killers"
,
"the lumineers"
,
"the offspring"
,
"the script"
,
"tiesto"
,
"tim mcgraw"
,
"toby keith"
,
"train"
,
"usher"
,
"vampire weekend"
,
"weezer"
,
"willie nelson"
,
"wiz khalifa"
,
"zac brown band"
,
"queens of the stone age"
,
"rascal flatts"
,
"rihanna"
,
"rob thomas"
,
"robin thicke"
,
"sebastian ingrosso"
,
"shakira"
,
"sheryl crow"
,
"smashing pumpkins"
,
"steve miller band"
,
"stone temple pilots"
,
"2 chainz"
,
"asap rocky"
,
"adam lambert"
,
"afrojack"
,
"against me"
,
"akon"
,
"alabama shakes"
,
"alesso"
,
"all american rejects"
,
"american authors"
,
"arctic monkeys"
,
"austin mahone"
,
"b.o.b."
,
"barenaked ladies"
,
"bb king"
,
"big sean"
,
"billy currington"
,
"boys like girls"
,
"brandon flowers"
,
"bret michaels"
,
"busta rhymes"
,
"cage the elephant"
,
"cake"
,
"carly rae jepsen"
,
"cat power"
,
"childish gambino"
,
"chris botti"
,
"chris cornell"
,
"ciara"
,
"clay aiken"
,
"clint black"
,
"coheed & cambria"
,
"colbie callait"
,
"collective soul"
,
"creedence clearwater revival"
,
"darius rucker"
,
"death cab for cutie"
,
"decemberists"
,
"dionne warwick"
,
"dispatch"
,
"disturbed"
,
"don williams"
,
"dropkick murphys"
,
"edward sharpe & ma"
,
"eli young band"
,
"erykah badu"
,
"feist"
,
"five finger death pur"
,
"g. love"
,
"gary allan"
,
"gavin degraw"
,
"godsmack"
,
"gogol bordello"
,
"goo goo dolls"
,
"gotye"
,
"gretchen wilson"
,
"grizzly bear"
,
"grouplove"
,
"gym class heroes"
,
"gypsy kings"
,
"hot chelle rae"
,
"incubus"
,
"india arie"
,
"james blunt"
,
"jason derulo"
,
"jewel"
,
"jill scott"
,
"josh turner"
,
"kc & sunshine band"
,
"kellie pickier"
,
"kelly rowland"
,
"kenny rogers"
,
"keri hilson"
,
"keyshia cole"
,
"korn"
,
"krewella"
,
"lana del ray"
,
"leann rimes"
,
"lee ann womack"
,
"lifehouse"
,
"lonestar"
,
"ludacris"
,
"lupe fiasco"
,
"mac miller"
,
"mgmt"
,
"nas"
,
"ne-yo"
,
"nerd"
,
"norah jones"
,
"oar"
,
"odd future"
,
"panic! at the disco"
,
"pat benatar"
,
"phillip phillips"
,
"rise against"
,
"sara bareilles"
,
"sara evans"
,
"seether"
,
"selena gomez"
,
"shinedown"
,
"skillet"
,
"snoop dog/lion"
,
"snow patrol"
,
"soulja boy"
,
"staind"
,
"steve aoki"
,
"steve martin & steep"
,
"taio cruz"
,
"taking back sunday"
,
"the civil wars"
,
"the roots"
,
"the wanted"
,
"third eye blind"
,
"three days grace"
,
"tori amos"
,
"travis tritt"
,
"trey songz"
,
"trisha year-wood"
,
"tyga"
,
"tyler the creator"
,
"wale"
,
"wyclef"
,
"young jeezy"
,
"young the giant"
,
"ziggy marley"
,
"2 door cinema club"
,
"30 seconds to mars"
,
"38 special"
,
"30h!3"
,
"a great big world"
,
"alex clare"
,
"all time low"
,
"andrew bird"
,
"andrew mcmachon"
,
"andy grammer"
,
"ani difranco"
,
"anna kendrick"
,
"anthony hamilton"
,
"atmosphere"
,
"awolnation"
,
"bastille"
,
"ben folds"
,
"big boi"
,
"big gigantic"
,
"billy ray cyrus"
,
"black crowes"
,
"blues traveler"
,
"broken social scene"
,
"bruce hornsby"
,
"capital cities"
,
"charlie daniels band"
,
"cheap trick"
,
"cher lloyd"
,
"chevelle"
,
"chris young"
,
"chromeo"
,
"chvrches"
,
"citizen"
,
"cobra starship"
,
"cold war kids"
,
"common"
,
"craig morgan"
,
"danny gokey"
,
"daryl worley"
,
"dashboard confessior"
,
"david archuletta"
,
"david crosby & grah"
,
"diamond rio"
,
"dj pauly d"
,
"eagles of death metal"
,
"eaton corbin"
,
"elvis crespo"
,
"faith evans"
,
"fat joe"
,
"fitz & the tantrums"
,
"flyleaf"
,
"fonseca"
,
"foreigner"
,
"french montana"
,
"future"
,
"girl talk"
,
"ice cube"
,
"ingrid michaelson"
,
"ja rule"
,
"jack' s mannequin"
,
"janelle monae"
,
"jay sean"
,
"jo dee messina"
,
"joan jett"
,
"john newman"
,
"john secada"
,
"juicy j"
,
"jurassic 5"
,
"justin moore"
,
"karmin"
,
"kelis"
,
"kenny wayne shepherd"
,
"lee brice"
,
"local natives"
,
"matisyahu"
,
"matt & kim"
,
"matt nathanson"
,
"michael franti"
,
"michelle branch"
,
"mighty mighty bosstones"
,
"miguel"
,
"mike posner"
,
"moe"
,
"needtobreathe"
,
"neon trees"
,
"nickel creek"
,
"ok go"
,
"olly murs"
,
"owl city"
,
"papa roach"
,
"phil vassar"
,
"plain white ts"
,
"puddle of mudd"
,
"redman/methodman"
,
"regina spektor"
,
"rick ross"
,
"rick springfield"
,
"santigold"
,
"sean kingston"
,
"sevendust"
,
"sevyn streeter"
,
"shedaisy"
,
"slash"
,
"slightly stoopid"
,
"smash mouth"
,
"sugar ray"
,
"sum 41"
,
"switchfoot"
,
"t-pain"
,
"terri clark"
,
"the bravery"
,
"the dandy warhols"
,
"the dream"
,
"the game"
,
"the national"
,
"the neighborhood"
,
"the starting line"
,
"the ting tings"
,
"the wallflowers"
,
"theory of a deadman"
,
"thompson square"
,
"timeflies"
,
"toby mac"
,
"tracy lawrence"
,
"travie mccoy"
,
"trombone shorty"
,
"walk the moon"
,
"21 pilots"
,
"3lau"
,
"aaron tippin"
,
"airborne toxic event"
,
"allen stone"
,
"aloe blacc"
,
"amerie"
,
"anberlin"
,
"andrew wk"
,
"ashanti"
,
"beirut"
,
"better than ezra"
,
"big bad vodoo dadd"
,
"big head todd & mo"
,
"blackberry smoke"
,
"blonde readhead"
,
"blue october"
,
"bob schneider"
,
"bone thugs & harmony"
,
"borgore"
,
"brandi carlisle"
,
"brett dennen"
,
"broken bells"
,
"c2c"
,
"carolina liar"
,
"chamillionaire"
,
"chiddy bang"
,
"chingy"
,
"chris cagie"
,
"chrisette michelle"
,
"christina milan"
,
"christina perri"
,
"circa survive"
,
"coolio"
,
"corey smith"
,
"da brat"
,
"david cook"
,
"devotchka"
,
"divine fits"
,
"dj shadow"
,
"drive by truckers"
,
"drop city yacht club"
,
"editors"
,
"everclear"
,
"fabolous"
,
"far east movement"
,
"feed me"
,
"finger eleven"
,
"flogging molly"
,
"fountains of wayne"
,
"galactic"
,
"gaslight anthem"
,
"george clinton"
,
"ghostface"
,
"gin blossoms"
,
"gloriana"
,
"goldfinger"
,
"gomez"
,
"grace potter & noctu"
,
"gucci mane"
,
"hanson"
,
"hoobastank"
,
"j. holiday"
,
"jack ingram"
,
"jadakiss"
,
"jakob dylan"
,
"jason michael carroll"
,
"jet"
,
"jhene aiko"
,
"john anderson"
,
"john popper project"
,
"julian marley"
,
"k' naan"
,
"kaiser chiefs"
,
"keith anderson"
,
"kentucky headhunter"
,
"kevin rudolf"
,
"kid ink"
,
"kongos"
,
"la roux"
,
"larry gatlin"
,
"lloyd"
,
"logic"
,
"lorrie morgan"
,
"los lobos"
,
"los lonely boys"
,
"lotus"
,
"love & theft"
,
"macy gray"
,
"mario"
,
"mark wills"
,
"mat kearney"
,
"michael kiwanuka"
,
"miike snow"
,
"mike gordon"
,
"minus the bear"
,
"mobb deep"
,
"mogwai"
,
"montell jordan"
,
"mord fustang"
,
"motion city soundtrack"
,
"musiq"
,
"mute math"
,
"naturally 7"
,
"nervo"
,
"nitty gritty dirt band"
,
"old 97's"
,
"ozomatli"
,
"p.o.d."
,
"pam tillis"
,
"paolo nutini"
,
"passenger"
,
"pat green"
,
"pepper"
,
"plies"
,
"public enemy"
,
"q-tip"
,
"r3hab"
,
"ray price"
,
"red"
,
"relient k"
,
"robert randolph"
,
"rufus wainright"
,
"sam moore"
,
"sammy adams"
,
"saving abel"
,
"say anything"
,
"schoolboy q"
,
"she wants revenge"
,
"sick puppies"
,
"silversun pickups"
,
"simple plan"
,
"sleigh bells"
,
"soul asylum"
,
"spearhead"
,
"spoon"
,
"st. vincent"
,
"stockholm syndrome"
,
"talib kweli"
,
"the temper trap"
,
"the wailers"
,
"three 6 mafia"
,
"toots & the maytals"
,
"travis porter"
,
"umphree's mcgee"
,
"vertical horizon"
,
"waka flocka flame"
,
"walk off the earth"
,
"white panda"
,
"wild cub"
,
"xzibit"
,
"yeasayer"
,
"yelawolf"
,
"yo gotti"
,
"a track"
,
"adventure club"
,
"alexis jordan"
,
"alien ant farm"
,
"andrew allen"
,
"arrested development"
,
"asher roth"
,
"augustana"
,
"baby bash"
,
"beenie man"
,
"ben kweller"
,
"ben rector"
,
"between the trees"
,
"biz markie"
,
"blake lewis"
,
"blaq audio"
,
"bobby valentino"
,
"bowling for soup"
,
"breathe carolina"
,
"brooke valentine"
,
"cash money"
,
"candlebox"
,
"cartel"
,
"charlie mars"
,
"chuck leavell"
,
"churchill"
,
"clap your hands say"
,
"clipse"
,
"cloud cult"
,
"clutch"
,
"colby odonis"
,
"colt ford"
,
"cowboy mouth"
,
"cracker"
,
"crystal bowersox"
,
"cults"
,
"cute is what we aim"
,
"danny brown"
,
"david banner"
,
"de la soul"
,
"default"
,
"disco biscuits"
,
"drowning pool"
,
"duncan sheik"
,
"dustin lynch"
,
"edwin mccain"
,
"emerson drive"
,
"eric hutchinson"
,
"eve 6"
,
"everlast"
,
"family force five"
,
"fastball"
,
"filter"
,
"five for fighting"
,
"flobots"
,
"forever the sickest k"
,
"four year strong"
,
"freelance whales"
,
"fuel"
,
"hal ketchum"
,
"hellogoodbye"
,
"holy ghost!"
,
"howie day"
,
"indigo girls"
,
"jake miller"
,
"james otto"
,
"jefferson starship"
,
"jeremih"
,
"jon mclaughlin"
,
"jonathan davis"
,
"josh gracin"
,
"josh thompson"
,
"joshua radin"
,
"junior doctor"
,
"justin nozuka"
,
"kaki king"
,
"kate voegele"
,
"king chip the ripper"
,
"kopecky family banc"
,
"kreayshawn"
,
"kris allen"
,
"le caslte vania"
,
"lee dewyze"
,
"less than jake"
,
"lights"
,
"lit"
,
"ludo"
,
"marc broussard"
,
"marc cohn"
,
"max weinberg big be"
,
"mayday parade"
,
"mims"
,
"mindy smith"
,
"n. miss all-stars"
,
"new boyz"
,
"noah & the whale"
,
"ofmontreal"
,
"parachute"
,
"pat travers"
,
"paul wall"
,
"perpetual groove"
,
"pleasure p"
,
"portugal the man"
,
"presidents of the usa"
,
"quiet riot"
,
"ra ra riot"
,
"raki m"
,
"rebirth brass band"
,
"red jumpsuit apparat"
,
"redlight king"
,
"reel big fish"
,
"rev theory"
,
"rhett atkins"
,
"rogue wave"
,
"rooney"
,
"rusted root"
,
"sea wolf"
,
"secondhand serenade"
,
"shawn mullins"
,
"shooter jennings"
,
"shwayze"
,
"sister hanl"
,
"slum village"
,
"solange knowles"
,
"st. lucia"
,
"state radio"
,
"steel train"
,
"steven marley"
,
"story of the year"
,
"sugarcult"
,
"july talk"
,
"the sheepdogs"
        };

    }
}
