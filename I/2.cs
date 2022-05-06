using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercici2
{
    class Program
    {
        public static hrEntities dE = new hrEntities();

        static void Main(string[] args)
        {
            bool final = false;

            do
            {
                               
                switch(menu())
                {
                    case 1:

                        altaFeina();

                        break;

                    case 2:

                        modificarTreballadors();

                        break;


                    case 0:

                        Console.WriteLine("Adeeu....");

                        final = true;

                        break;
                }

                wc();

            } while (!final);

        }



        //Opcio 1
        public static void altaFeina()
        {

            Console.WriteLine("Donar d'alta una nova feina");

            Console.WriteLine("Com es dira la nova feina?");
                string nom = Console.ReadLine();

            //comprovo si el nom existeix
            int existeix = dE.jobs.Where(j => j.job_title.ToUpper() == nom.ToUpper()).Count();

            if(existeix == 0)
            {
                int min, max;

                string msg = "Entra el salari minim de la feina";
                string msg2 = "I ara el salari maxim de la feina";
                string err = "Has d'entrar un numero";


                //demano el min i max salari
                min = demanarInt(msg, err);
                max = demanarInt(msg2, err);

                //creo la feina
                jobs j = new jobs();

                j.job_title = nom;
                j.max_salary = max;
                j.min_salary = min;

                dE.jobs.Add(j);
                dE.SaveChanges();


            }
            else
            {
                Console.WriteLine("Ja existeix una feina amb aquest nom.");
            }

        }


        //Opcio 2
        public static void modificarTreballadors()
        {
            Console.WriteLine("Canviar feina als treballadors");

            string feina_vella, feina_nova;

            Console.WriteLine("Quina feina estan fent ara?");
                feina_vella = Console.ReadLine();
            
            //comprovo que existeixi la feina vella
            int existeix = dE.jobs.Where(j => j.job_title.ToUpper() == feina_vella.ToUpper()).Count();

            if (existeix == 1)
            {
                //obtinc l'id de la feina vella
                int id_vell = dE.jobs.Where(j => j.job_title.ToUpper() == feina_vella.ToUpper()).FirstOrDefault().job_id;

                Console.WriteLine("A quina feina els vols passar?");
                    feina_nova = Console.ReadLine();

                //comprovo que existeixi la feina nova
                int existeix2 = dE.jobs.Where(j => j.job_title.ToUpper() == feina_nova.ToUpper()).Count();

                if (existeix2 == 1)
                {
                    //obtinc l'id de la feina nova
                    int id_nou = dE.jobs.Where(j => j.job_title.ToUpper() == feina_nova.ToUpper()).FirstOrDefault().job_id;


                    //obtinc els empleats amb la feina vella
                    List<employees> emp_list = dE.employees.Where(e => e.job_id == id_vell).ToList();


                    //i els hi assigno la nova feina
                    foreach(employees e in emp_list)
                    {
                        e.job_id = id_nou;
                    }

                    //i deso els canvis
                    dE.SaveChanges();


                    Console.WriteLine("Treballadors modificats");

                }
                else
                {
                    Console.WriteLine("No hi ha cap feina amb aquest nom");
                }

            }
            else
            {
                Console.WriteLine("No hi ha cap feina amb aquest nom");
            }

        }


/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public static int menu()
        {
            int opcio = -1;
            do
            {
                Console.WriteLine("Menu:\n");

                Console.WriteLine("1-. Donar d'alta una feina");
                Console.WriteLine("2-. Modificar treballadors");
                Console.WriteLine("0-. Sortir\n");

                Console.Write("Escull una opcio: ");

                if (!int.TryParse(Console.ReadLine(), out opcio) || opcio < 0 || opcio > 2)
                {
                    Console.WriteLine("Entra un numero de 0 al 2.");
                    wc();
                }

            } while (opcio < 0 || opcio > 2);

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
