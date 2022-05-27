using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Driver;
using MongoDB.Bson;

namespace Exercici_2
{
    class Program
    {
        public static MongoClient dbClient;
        public static IMongoDatabase db;

        static void Main(string[] args)
        {
            bool final = false;

            connectDB();
            do
            {
                switch (menu())
                {
                    case 1:
//● Llistar les dades personals de les persones de la plantilla de l’empresa municipal de transport urbà (persones que venen bitllets a l’estació, persones que condueixen un bus urbà i persones que s’encarreguen del manteniment). (1,5 punts)

                        llistarPersonal();

                        break;
                    case 2:
//● Llistar les línies disponibles. (1 punt)

                        llistarLinies();

                        break;
                    case 3:
//● Llistar l’històric de preus. (1 punt)

                        llistarPreus();

                        break;
                    case 4:
//● Llistar les línies ordenades de més a menys utilitzada. (2 punts)

                        llistarLiniesUs();

                        break;
                    case 5:
//● Donada una data i una línia, el número de passatgers que han utilitzat la línia durant la data indicada. (2 punts)

                        passatgersLinia();

                        break;
                    case 6:
//● Donades dues dates, els ingressos de les diferents línies durant les dates indicades. (2,5 punts)

                        ingressosLinies();

                        break;
                    case 0:

                        Console.WriteLine("Adeu...");
                        final = true;


                        break;

                }
                wc();


            } while (!final);




        }

        public static void connectDB()
        {
            var settings = MongoClientSettings.FromConnectionString("mongodb+srv://aprat:1234@mongoproject.hxqyz.mongodb.net/myFirstDatabase?retryWrites=true&w=majority");
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            dbClient = new MongoClient(settings);
            db = dbClient.GetDatabase("M06_UF3_A01_MongoDB");
        }

        public static List<BsonDocument> ObtenirRegistres(string collection)
        {
            IMongoCollection<BsonDocument> mongoCollection = db.GetCollection<BsonDocument>(collection);

            var sort = Builders<BsonDocument>.Sort.Ascending("data");

            return mongoCollection.Find(new BsonDocument()).Sort(sort).ToList();
        }

        public static IMongoCollection<BsonDocument> ObtenirCollection(string collection)
        {
            return db.GetCollection<BsonDocument>(collection);
        }




        //● Llistar les dades personals de les persones de la plantilla de l’empresa municipal de transport urbà (persones que venen bitllets a l’estació, persones que condueixen un bus urbà i persones que s’encarreguen del manteniment). (1,5 punts)

        public static void llistarPersonal()
        {
            Console.WriteLine("Llistat del personal: ");

            List<BsonDocument> personal = ObtenirRegistres("treballadors");//agafo tots els registres de treballadors

            foreach(BsonDocument p in personal)
            {
                Console.WriteLine("Treballador Nº: {0}, Nom: {1}, Feina: {2}", p["id"], p["nom"], p["feina"]);
            }
        }

//● Llistar les línies disponibles. (1 punt)

        public static void llistarLinies()
        {
            Console.WriteLine("Llistat de les linies: ");

            List<BsonDocument> linies = ObtenirRegistres("linies");//agafo tots els registres de linies

            foreach (BsonDocument l in linies)
            {
                Console.WriteLine("Linea Nº: {0}, Nom: {1}", l["id"], l["nom"]);
            }
        }

        //● Llistar l’històric de preus. (1 punt)

        public static void llistarPreus()
        {
            Console.WriteLine("Llistat de preus: ");

            List<BsonDocument> preus = ObtenirRegistres("preus");//agafo tots els registres de preus

            foreach (BsonDocument p in preus)
            {

                Console.WriteLine("Preu Nº: {0}, Linea Nº: {1}, Import: {2}, Data de vigencia: {3}", p["id"], p["linia_id"], p["import"], p["data"]);
            }
        }

//● Llistar les línies ordenades de més a menys utilitzada. (2 punts)

        public class ViatgesLinia : IComparable<ViatgesLinia>   //Aquesta classe em servira per poder comparar i ordenar despres
        {                                                       //els viatges de les diferents linies
            public string nom_linia { get; set; }
            public int num_viatges { get; set; }

            public ViatgesLinia()
            {
                nom_linia = "";
                num_viatges = 0;
            }

            public ViatgesLinia(string l, int v)
            {
                nom_linia = l;
                num_viatges = v;
            }

            int IComparable<ViatgesLinia>.CompareTo(ViatgesLinia other)
            {
                if(other == null)
                    return 1;
                else
                    return this.num_viatges.CompareTo(other.num_viatges);
            }
        }

        public static void llistarLiniesUs()
        {
            Console.WriteLine("Linies ordenades de menys a mes utilitzada");

            IMongoCollection<BsonDocument> dbViatges = ObtenirCollection("viatges");    //agafo la col·leccio de viatges, sense transformar-ho en llista per poder filtrar
                                                                                        //els registres

            List<BsonDocument> linies = ObtenirRegistres("linies");                     //agafo tots els registres de linies

            List<ViatgesLinia> vl = new List<ViatgesLinia>();

            foreach(BsonDocument l in linies)//itero per les linies mentre agafo el seu nom i compto quants viatges han fet per linia
            {
                var fil = Builders<BsonDocument>.Filter.Where(f => f["linia_id"] == l["id"]);
                List<BsonDocument> viatges_linia = dbViatges.Find(fil).ToList();
                vl.Add(new ViatgesLinia(l["nom"].ToString(), viatges_linia.Count()));
            }

            vl.Sort();//Ordeno amb la condicio declarada a la classe

            foreach (ViatgesLinia v in vl)
            {
                Console.WriteLine("Linia: {0}, Viatges: {1}", v.nom_linia, v.num_viatges);
            }
        }

//● Donada una data i una línia, el número de passatgers que han utilitzat la línia durant la data indicada. (2 punts)

        public static void passatgersLinia()
        {
            Console.WriteLine("Numero d'usuaris que utilitzen una linia un dia.");

            Console.WriteLine("Quina linia vols cercar.");
            string nom_linia = Console.ReadLine();

            int id_linia = 0;

            List<BsonDocument> linies = ObtenirRegistres("linies");

            foreach (BsonDocument l in linies)//itero per les linies per comprovar si existeix el nom de la linia i agafar l'id si existeix
            {
                if (l["nom"] == nom_linia)
                    id_linia = int.Parse(l["id"].ToString());
            }

            if(id_linia != 0)
            {
                List<BsonDocument> viatges = ObtenirRegistres("viatges");

                string msg = "Quina data vols cercar.";
                string err = "Entra una data valida.";
                DateTime data = demanarDateTime(msg, err);

                List<BsonDocument> viatges2 = viatges.FindAll(v => int.Parse(v["linia_id"].ToString()) == id_linia & DateTime.Parse(v["data"].ToString()) == data);//Filtro els viatges per linia_id i amb la data donada

                Console.WriteLine("El dia {0}, han usat la linia {1} {2} passatgers", data.ToString("dd/MM/yyyy"), nom_linia, viatges2.Count());

            }
            else
            {
                Console.WriteLine("No existeix cap linia amb aquets nom");
            }
        }

        //● Donades dues dates, els ingressos de les diferents línies durant les dates indicades. (2,5 punts)

        public static void ingressosLinies()
        {
            Console.WriteLine("Aqui llistarem els ingressos de les linies a partir de dues dates.\n");

            string msg = "Escriu la data per la que vols començar a buscar";
            string msg2 = "Escriu la data fina a la que vols buscar";
            string err = "Escriu una data valida";

            DateTime data_inici = demanarDateTime(msg, err);
            DateTime data_final = demanarDateTime(msg2, err);


            List<BsonDocument> linies = ObtenirRegistres("linies");
            List<BsonDocument> viatges = ObtenirRegistres("viatges");
            List<BsonDocument> preus = ObtenirRegistres("preus");

            List<BsonDocument> viatges_filtrats = viatges.FindAll(v => DateTime.Parse(v["data"].ToString()) > data_inici & DateTime.Parse(v["data"].ToString()) < data_final);//Filtro els viatges que estan dins de les dates indicades 

            foreach (BsonDocument l in linies)
            {
                int ingressos_linia = 0;

                List<BsonDocument> viatges_linia = viatges_filtrats.FindAll(v => v["linia_id"] == l["id"]);//Separo per linies

                List<BsonDocument> preus_linia = preus.FindAll(p => p["linia_id"].ToString() == l["id"].ToString());

                foreach(BsonDocument v in viatges_linia)                        //Comprovo si la data del viatge (v["data"]) esta entre la data del preu de la linia (preu_linia[x]["data"])
                {                                                               //i el seguent preu d'aquella linia (preu_linia[i+1]["data"]),
                    int i = 0;                                                  //si es al mig agafo el preu d'aquest preu_linia (preu_linia[i+1]["import"]) i el sumo als ingressos
                    bool trobat = false;                                        //Si arriba a l'ultim registre dels preu_viatge el que comprovo es que la data del viatge (v["data"]) sigui mes gran que la data del preu_linia (preu_linia[x]["data"])

                    
                    while (i < preus_linia.Count() && !trobat)
                    {
                        if (i != preus_linia.Count() - 1 && i < preus_linia.Count() - 1)
                        {
                            if(DateTime.Parse(v["data"].ToString()) > DateTime.Parse(preus_linia[i]["data"].ToString()) && DateTime.Parse(v["data"].ToString()) > DateTime.Parse(preus_linia[i + 1]["data"].ToString()))    
                            {                                                                                                                                                                                               
                                ingressos_linia += int.Parse(preus_linia[i]["import"].ToString());                                                                                                                          
                                trobat = !trobat;                                                                                                                                                                           
                                                                                                                                                                                                                            
                            }
                        }
                        else
                        {
                            if (DateTime.Parse(v["data"].ToString()) > DateTime.Parse(preus_linia[i]["data"].ToString()))
                            {
                                ingressos_linia += int.Parse(preus_linia[i]["import"].ToString());
                                trobat = !trobat;
                            }
                        }
                        i++;
                    }
                }
                Console.WriteLine("INGRESSOS: La linia {0} ha tingut un ingressos de {1} entre les dates {2} i {3}", l["nom"], ingressos_linia, data_inici.ToString("dd/MM/yyyy"), data_final.ToString("dd/MM/yyyy"));
            }
        }

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static int menu()
        {
            int opcio = -1;
            do
            {
                Console.WriteLine("Menu:\n");

                Console.WriteLine("1-. Llistar les dades personals de les persones de la plantilla de l’empresa municipal de transport urbà");
                Console.WriteLine("2-. Llistar les línies");
                Console.WriteLine("3-. Llistar l’històric de preus");
                Console.WriteLine("4-. Llistar les línies ordenades de més a menys utilitzada");
                Console.WriteLine("5-. El número de passatgers que han utilitzat la línia durant la data indicada");
                Console.WriteLine("6-. Els ingressos de les diferents línies durant les dates indicades");
                Console.WriteLine("0-. Sortir\n");

                Console.Write("Escull una opcio: ");

                if (!int.TryParse(Console.ReadLine(), out opcio) || opcio < 0 || opcio > 6)
                {
                    Console.WriteLine("Entra un numero de 0 al 6.");
                    wc();
                }

            } while (opcio < 0 || opcio > 6);

            return opcio;
        }
        public static void wc()
        {
            Console.WriteLine("Prem qualsevol tecla per continuar");
            Console.ReadKey();

            Console.Clear();
        }
        /*public static void wc2()
        {
            Console.WriteLine("Prem qualsevol tecla per veure mes resultats");
            Console.ReadKey();

            Console.Clear();
        }*/

        public static int demanarInt(string missatge, string error)
        {
            int num;
            bool num_ok = false;

            do
            {
                Console.WriteLine(missatge);

                if (!int.TryParse(Console.ReadLine(), out num))
                {
                    Console.WriteLine(error);
                    wc();
                }
                else
                {
                    Console.WriteLine(num);
                    num_ok = true;
                }

            } while (!num_ok);

            return num;
        }
        public static DateTime demanarDateTime(string missatge, string error)
        {
            DateTime data;
            bool date_ok = false;

            do
            {
                Console.WriteLine(missatge);

                if (!DateTime.TryParse(Console.ReadLine(), out data))
                {
                    Console.WriteLine(error);
                    wc();
                }
                else
                {
                    Console.WriteLine(data);
                    date_ok = true;
                }

            } while (!date_ok);

            return data;
        }
    }
}
