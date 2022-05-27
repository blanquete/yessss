using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace UF3_A01_Mongo_CBancaria
{
    class Program
    {
        public static MongoClient dbClient;
        public static IMongoDatabase db;
        static void Main(string[] args)
        {
            ConectarBBDD();
            bool salir = false;
            do
            {
                switch (menu())
                {
                    case 1:
                        listarClientes();
                        break;
                    case 2:
                        listarCuentas();
                        break;
                    case 3:
                        ingresarDineroCuenta();
                        break;
                    case 4:
                        retirarDineroCuenta();
                        break;
                    case 5:
                        transferencias();
                        break;
                    case 6:
                        historial();
                        break;
                    case 7:
                        salir = true;
                        break;
                }
            } while (!salir);
            
        }

        //Menú del programa.
        private static int menu()
        {
            int aux = -1;
            try
            {
                Console.WriteLine("----- Cuenta Bancaria -----");
                Console.WriteLine();
                Console.WriteLine("1. Llistar les dades personals de tots els clients.");
                Console.WriteLine("2. LLlistar els comptes corrents dels que disposa una persona amb el seu saldo.");
                Console.WriteLine("3. Ingressar diners a un compte corrent. ");
                Console.WriteLine("4. Retirar diners d’un compte corrent.");
                Console.WriteLine("5. Realitzar una transferència de diners d’un compte corrent d’una persona a un compte corrent d’una altre persona o de la mateixa. ");
                Console.WriteLine("6. Llistar l’historial d’operacions d’un compte corrent.");
                Console.WriteLine("7. Salir.");
                Console.WriteLine();
                Console.Write("Elige una opcion: ");
                int.TryParse(Console.ReadLine(), out aux);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hay un problema con el código. {ex}");
            }
            return aux;
        }
        private static int subMenu()
        {
            int aux = 0;
            Console.WriteLine();
            Console.WriteLine("Menu de opciones.");
            Console.WriteLine("-".PadLeft(30,'-'));

            Console.WriteLine("1. Ingresar dinero en una de tus otras cuentas.");
            Console.WriteLine("2. Ingresar el dinero a otra persona.");
            Console.WriteLine();
            Console.Write("Elige una opción: ");
            int.TryParse(Console.ReadLine(), out aux);

            return aux;
        }
        private static void transferencias()
        {
            limpiar();
            IMongoCollection<BsonDocument> coleccion = db.GetCollection<BsonDocument>("cliente");
            List<BsonDocument> lista = obtenerLista("cliente");
            var aux = 0;
            int aux_cuenta = 0, aux_cuenta2 = 0, cantidad = 0 ;

            Console.WriteLine("Introduce su NIF");
            string aux_NIF = Console.ReadLine();
            string IBAN_destinatario = " ";
            string IBAN_origen = " ";

            
            foreach (BsonDocument item in lista)
            {
                if(item["NIF"].ToString() == aux_NIF)
                {
                    Console.WriteLine("Estas son las cuentas que tienes.");
                    Console.WriteLine("-".PadLeft(40,'-'));
                    foreach (var item2 in item["cuentas"].AsBsonArray)
                    {
                        Console.WriteLine($"ID: {item2["id"].ToString()}  NOMBRE: {item2["nombre_Cuenta"].ToString().PadRight(5)}  SALDO: {item2["saldo"].ToString()} ");
                    }

                    switch (subMenu())
                    {
                        case 1:
                            Console.WriteLine("Introduce el ID cuenta origen.");
                            int.TryParse(Console.ReadLine(), out aux_cuenta);
                            Console.WriteLine("Introduce el ID cuenta destino.");
                            int.TryParse(Console.ReadLine(), out aux_cuenta2);

                            if(comprobarCuenta(aux_NIF, aux_cuenta) == true || comprobarCuenta(aux_NIF, aux_cuenta2)== true)
                            {
                                Console.WriteLine("Introduce la cantidad que quieres traspasar: ");
                                int.TryParse(Console.ReadLine(), out cantidad);

                                retirarDineroMismaCuenta(cantidad, aux_cuenta, aux_NIF);
                                sumarDineroMismaCuenta(cantidad, aux_cuenta2, aux_NIF);

                                Console.WriteLine("Traspado de dinero a otra cuenta realizada.");
                            }
                            else
                            {
                                Console.WriteLine("Problemas.");
                            }

                            break;
                        case 2:

                            Console.Write("Introduce el IBAN  de tu cuenta: ");
                            IBAN_origen = Console.ReadLine();

                            Console.Write("Introduce el IBAN del destinatario: ");
                            IBAN_destinatario = Console.ReadLine();

                            // ES66 2100 0418 4012 3456 7891
                            Console.WriteLine();
                            if(comprobarIBAN(IBAN_destinatario) == true || comprobarIBAN(IBAN_origen)== true)
                            {
                                Console.WriteLine("Destinatario encontrado. \n");

                                Console.WriteLine("Introduce la cantidad que quieres traspasar: ");
                                int.TryParse(Console.ReadLine(), out cantidad);

                                retirarCuentaTransferencia(IBAN_origen, cantidad);
                                sumarCuentaTransferencia(IBAN_destinatario, cantidad);

                                Console.WriteLine("Transferencia realizada.");
                            }
                            else
                            {
                                Console.WriteLine("El IBAN introducido no corresponde a ningún cliente.");
                            }

                            break;
                    }

                }
            }


            limpiar();
        }
        private static bool comprobarCuenta(string nif , int cuenta)
        {
            List<BsonDocument> lista = obtenerLista("cliente");

            foreach (BsonDocument item in lista)
            {
                item["NIF"] = nif;
                foreach (var item2 in item["cuentas"].AsBsonArray)
                {
                    if((int)item2["id"] == cuenta)
                    {
                        return true;
                    }
                    else { return false; }
                }
            }
            return true;
        }
        private static void historial()
        {
            limpiar();
            bool encontrar = false;
            List<BsonDocument> lista = obtenerLista("historial");

            Console.WriteLine("Introduce su IBAN para ver su historial");
            string aux_IBAN = Console.ReadLine();

            Console.WriteLine();

            Console.Write("Descripción".PadRight(19) + "Operación" + "\n");
            Console.WriteLine("-".PadLeft(40, '-'));

            foreach (BsonDocument item in lista)
            {
                if (item["IBAN"].ToString() == aux_IBAN)
                {
                    Console.WriteLine(item["descripcion"].ToString().PadRight(19) +  item["operacion"].ToString());
                    encontrar = true;
                }
            }
            
            if (!encontrar)
            {
                Console.WriteLine($"El IBAN {aux_IBAN} introducido no existe.");
            }
            else
            {
                Console.WriteLine("Fin del historial.");
            }

            limpiar();
        }
        private static bool comprobarIBAN(string IBAN)
        {
            List<BsonDocument> lista = obtenerLista("cliente");

            foreach (var item in lista)
            {
                foreach (var item2 in item["cuentas"].AsBsonArray)
                {
                    if(item2["IBAN"] == IBAN)
                    {
                        return true;
                    }else { return false; }
                }
            }
            return true;
        }
        private static void retirarCuentaTransferencia(string IBAN, int cantidad)
        {
            bool encontrar = false;
            var aux = 0;
            var aux_Iban = "";
            List<BsonDocument> lista = obtenerLista("cliente");

            IMongoCollection<BsonDocument> coleccion = db.GetCollection<BsonDocument>("cliente");
            IMongoCollection<BsonDocument> coleccionHistorial = db.GetCollection<BsonDocument>("historial");

            //El ingreso tenemos que hacer dentro del foreach para poder verificar si el cliente existe con su NIF.
            foreach (var item in lista)
            {
                foreach (var item2 in item["cuentas"].AsBsonArray)
                {
                    //Buscamos por cuenta para realizar la operación.
                    if (item2["IBAN"] == IBAN && !encontrar)
                    {
                        aux = int.Parse(item2["saldo"].ToString());
                        aux -= cantidad;
                        aux_Iban = item2["IBAN"].ToString();
                        encontrar = true;
                    }
                }
            }

            //Realizamos el update.
            var arrayFilter = Builders<BsonDocument>.Filter.Eq("cuentas.IBAN", IBAN);
            var arrayUpdate = Builders<BsonDocument>.Update.Set("cuentas.$.saldo", aux);

            coleccion.UpdateOne(arrayFilter, arrayUpdate);

            var document = new BsonDocument
                {
                    {"NIF", "null"},
                    {"IBAN", aux_Iban},
                    {"descripcion", "Dinero retirado para transferencia."},
                    {"fecha", DateTime.UtcNow},
                    {"operacion", cantidad}
                };
            coleccionHistorial.InsertOne(document);

        }
        private static void sumarCuentaTransferencia(string IBAN, int cantidad)
        {
            bool encontrar = false;
            var aux = 0;
            var aux_Iban = "";
            List<BsonDocument> lista = obtenerLista("cliente");

            IMongoCollection<BsonDocument> coleccion = db.GetCollection<BsonDocument>("cliente");
            IMongoCollection<BsonDocument> coleccionHistorial = db.GetCollection<BsonDocument>("historial");

            //El ingreso tenemos que hacer dentro del foreach para poder verificar si el cliente existe con su NIF.
            foreach (var item in lista)
            {
                foreach (var item2 in item["cuentas"].AsBsonArray)
                {
                    //Buscamos por cuenta para realizar la operación.
                    if (item2["IBAN"] == IBAN && !encontrar)
                    {
                        aux = int.Parse(item2["saldo"].ToString());
                        aux += cantidad;
                        aux_Iban = item2["IBAN"].ToString();
                        encontrar = true;
                    }
                }
            }
            //Realizamos el update.
            var arrayFilter = Builders<BsonDocument>.Filter.Eq("cuentas.IBAN", IBAN);
            var arrayUpdate = Builders<BsonDocument>.Update.Set("cuentas.$.saldo", aux);

            coleccion.UpdateOne(arrayFilter, arrayUpdate);

            var document = new BsonDocument
            {
                {"NIF", "null"},
                {"IBAN", aux_Iban},
                {"descripcion", "Transferencia hecha."},
                {"fecha", DateTime.UtcNow},
                {"operacion", cantidad}
            };
            coleccionHistorial.InsertOne(document);
        }
        private static void sumarDineroMismaCuenta(int cantidad, int cuenta, string NIF)
        {
            var aux = 0;
            var aux_Iban = "";
            List<BsonDocument> lista = obtenerLista("cliente");

            var aux_nif = NIF;

            IMongoCollection<BsonDocument> coleccion = db.GetCollection<BsonDocument>("cliente");
            IMongoCollection<BsonDocument> coleccionHistorial = db.GetCollection<BsonDocument>("historial");

            //El ingreso tenemos que hacer dentro del foreach para poder verificar si el cliente existe con su NIF.
            foreach (var item in lista)
            {
                if (aux_nif == item["NIF"].ToString())
                {
                    //Busco el saldo de la cuenta a partir del código.
                    foreach (var item2 in item["cuentas"].AsBsonArray)
                    {
                        //Buscamos por cuenta para realizar la operación.
                        if(item2["id"] == cuenta)
                        {
                            aux = int.Parse(item2["saldo"].ToString());
                            aux += cantidad;
                            aux_Iban = item2["IBAN"].ToString();
                        }
                    }

                    //Realizamos el update.
                    var arrayFilter = Builders<BsonDocument>.Filter.Eq("NIF", aux_nif)
                       & Builders<BsonDocument>.Filter.Eq("cuentas.id", cuenta);

                    var arrayUpdate = Builders<BsonDocument>.Update.Set("cuentas.$.saldo", aux);

                    coleccion.UpdateOne(arrayFilter, arrayUpdate);

                    var document = new BsonDocument
                    {
                        {"NIF", aux_nif},
                        {"IBAN", aux_Iban},
                        {"descripcion", "Obtener dinero de otra cuenta propia."},
                        {"fecha", DateTime.UtcNow},
                        {"operacion", cantidad}
                    };
                    coleccionHistorial.InsertOne(document);
                }
            }
        }
        private static void retirarDineroMismaCuenta(int cantidad, int cuenta, string NIF)
        {
            var aux = 0;
            var aux_Iban = "";
            List<BsonDocument> lista = obtenerLista("cliente");

            var aux_nif = NIF;

            IMongoCollection<BsonDocument> coleccion = db.GetCollection<BsonDocument>("cliente");
            IMongoCollection<BsonDocument> coleccionHistorial = db.GetCollection<BsonDocument>("historial");

            //El ingreso tenemos que hacer dentro del foreach para poder verificar si el cliente existe con su NIF.
            foreach (var item in lista)
            {
                if (aux_nif == item["NIF"].ToString())
                {
                    //Busco el saldo de la cuenta a partir del código.
                    foreach (var item2 in item["cuentas"].AsBsonArray)
                    {
                        //Buscamos por cuenta para realizar la operación.
                        if (item2["id"] == cuenta)
                        {
                            aux = int.Parse(item2["saldo"].ToString());
                            aux -= cantidad;
                            aux_Iban = item2["IBAN"].ToString();
                        }
                    }

                    //Realizamos el update.
                    var arrayFilter = Builders<BsonDocument>.Filter.Eq("NIF", aux_nif)
                       & Builders<BsonDocument>.Filter.Eq("cuentas.id", cuenta);

                    var arrayUpdate = Builders<BsonDocument>.Update.Set("cuentas.$.saldo", aux);

                    coleccion.UpdateOne(arrayFilter, arrayUpdate);

                    var document = new BsonDocument
                    {
                        {"NIF", aux_nif},
                        {"IBAN", aux_Iban},
                        {"descripcion", "Retirar Dinero a otra cuenta propia."},
                        {"fecha", DateTime.UtcNow},
                        {"operacion", cantidad}
                    };
                    coleccionHistorial.InsertOne(document);
                }
            }
        }
        private static void retirarDineroCuenta()
        {
            limpiar();

            bool encontrar = false;
            var aux = 0;
            var aux_Iban = "";
            List<BsonDocument> lista = obtenerLista("cliente");

            Console.WriteLine("Ingrese su NIF");
            var aux_nif = Console.ReadLine();

            IMongoCollection<BsonDocument> coleccion = db.GetCollection<BsonDocument>("cliente");
            IMongoCollection<BsonDocument> coleccionHistorial = db.GetCollection<BsonDocument>("historial");

            //El ingreso tenemos que hacer dentro del foreach para poder verificar si el cliente existe con su NIF.
            foreach (var item in lista)
            {
                if (aux_nif == item["NIF"].ToString() && !encontrar)
                {
                    Console.WriteLine("Introduce la cantidad de dinero que desea retirar");
                    int aux_cantidad = int.Parse(Console.ReadLine());

                    Console.WriteLine("En que cuenta quieres retirar el dinero");
                    var aux_cuenta = int.Parse(Console.ReadLine());

                    //Busco el saldo de la cuenta a partir del código y se incrementa.
                    foreach (var item2 in item["cuentas"].AsBsonArray)
                    {
                        aux = int.Parse(item2["saldo"].ToString());
                        aux -= aux_cantidad;
                        aux_Iban = item2["IBAN"].ToString();
                    }

                    //Realizamos el update.
                    var arrayFilter = Builders<BsonDocument>.Filter.Eq("NIF", aux_nif)
                       & Builders<BsonDocument>.Filter.Eq("cuentas.id", aux_cuenta);

                    var arrayUpdate = Builders<BsonDocument>.Update.Set("cuentas.$.saldo", aux);

                    coleccion.UpdateOne(arrayFilter, arrayUpdate);

                    var document = new BsonDocument
                    {
                        {"NIF", aux_nif},
                        {"IBAN", aux_Iban},
                        {"descripcion", "Retirar"},
                        {"fecha", DateTime.UtcNow},
                        {"operacion", aux_cantidad}
                    };
                    coleccionHistorial.InsertOne(document);

                    encontrar = true;
                }
            }

            if (!encontrar)
            {

                Console.WriteLine($"El NIF {aux_nif} no corresponde a ningún cliente");
            }
            else
            {
                Console.WriteLine("Dinero retirado");
            }

            limpiar();
        }
        private static void ingresarDineroCuenta()
        {
            limpiar();

            bool encontrar = false;
            var aux = 0;
            var aux_IBAN = "";
            List<BsonDocument> lista = obtenerLista("cliente");

            Console.WriteLine("Ingrese su NIF");
            var aux_nif = Console.ReadLine();

            //El ingreso tenemos que hacer dentro del foreach para poder verificar si el cliente existe con su NIF.

            IMongoCollection<BsonDocument> coleccion = db.GetCollection<BsonDocument>("cliente");
            IMongoCollection<BsonDocument> coleccionHistorial = db.GetCollection<BsonDocument>("historial");
            
            foreach (var item in lista)
            {
                if (aux_nif == item["NIF"].ToString() && !encontrar)
                {
                    Console.WriteLine("Introduce la cantidad de dinero que desea introducir");
                    int aux_cantidad = int.Parse(Console.ReadLine());

                    Console.WriteLine("En que cuenta quieres introducir el dinero");
                    var aux_cuenta = int.Parse(Console.ReadLine());

                    //Busco el saldo de la cuenta a partir del código y se incrementa.
                    foreach (var item2 in item["cuentas"].AsBsonArray)
                    {
                        aux = int.Parse(item2["saldo"].ToString());
                        aux += aux_cantidad;
                        aux_IBAN = item2["IBAN"].ToString();
                    }

                    //Realizamos el update.
                    var arrayFilter = Builders<BsonDocument>.Filter.Eq("NIF", aux_nif)
                       & Builders<BsonDocument>.Filter.Eq("cuentas.id", aux_cuenta);

                    var arrayUpdate = Builders<BsonDocument>.Update.Set("cuentas.$.saldo", aux);

                    coleccion.UpdateOne(arrayFilter, arrayUpdate);

                    var document = new BsonDocument
                    {
                        {"NIF", aux_nif},
                        {"IBAN", aux_IBAN},
                        {"descripcion", "Ingresar"},
                        {"fecha", DateTime.UtcNow},
                        {"operacion", aux_cantidad}
                    };
                    coleccionHistorial.InsertOne(document);

                    encontrar = true;
                }
            }

            if (!encontrar) {

                Console.WriteLine($"El NIF {aux_nif} no corresponde a ningún cliente");
            }
            else
            {
                Console.WriteLine("Dinero ingresado");
            }


            limpiar();
        }
        private static void listarCuentas()
        {
            limpiar();

            List<BsonDocument> lista = obtenerLista("cliente");

            Console.WriteLine("Introduce el NIF del cliente");
            string aux_nif = Console.ReadLine();

            if(lista.Count > 0)
            {
                //Título.
                Console.WriteLine($"Id".PadRight(6) + "NIF".PadRight(11) + "Nombre".PadRight(11) + "Nombre de cuenta".PadRight(8) + "Saldo".PadLeft(8));
                //Muestra todos los caracteres necesarios -.
                Console.WriteLine("-".PadLeft(60, '-'));

                foreach (BsonDocument item in lista)
                {
                    if (item["NIF"].ToString() == aux_nif)
                    {
                        //Función para poder recorrer el array de cuentas.
                        foreach (var item2 in item["cuentas"].AsBsonArray)
                        {
                            Console.WriteLine($"{item2["id"].ToString().PadRight(5)} {item["NIF"].ToString().PadLeft(4)} {item["nombre"].ToString().PadLeft(5)} " +
                                $"{item2["nombre_Cuenta"].ToString().PadLeft(11)}   {item2["saldo"].ToString().PadLeft(16)}");
                        }
                    }
                }
            }
            Console.WriteLine();
            limpiar();
        }
        private static void listarClientes()
        {
            limpiar();
            //Obtenemos la tabla cliente, añadiendo el string correspondiente.
            List<BsonDocument> listClient = obtenerLista("cliente");

            //Cuenta cuantos registros tenemos asociados.
            if(listClient.Count > 0)
            {
                //Título.
                Console.WriteLine($"Id".PadRight(6) + "NIF".PadRight(10) +  "Nombre".PadRight(10) + "Apellidos".PadLeft(8));
                //Muetra todos los caracteres necesarios -.
                Console.WriteLine("-".PadLeft(46,'-'));

                //Recorremos toda la tabla.
                //Vamos mostrando los valores de la tabla que nosotros queremos mostrar.
                foreach (BsonDocument item in listClient)
                {
                    Console.WriteLine($"{item["id"].ToString().PadRight(3)} {item["NIF"].ToString().PadRight(10)}" +
                        $" {item["nombre"].ToString().PadRight(10)} {item["apellidos"].ToString().PadLeft(8)} ");
                }
            }
            limpiar();
        }
        //Obtenemos la tabla o colección, con el nombre correspondiente.
        private static List<BsonDocument> obtenerLista(string nombre)
        {
            IMongoCollection<BsonDocument> coleccion = db.GetCollection<BsonDocument>(nombre);
            List<BsonDocument> lista = coleccion.Find(new BsonDocument()).ToList();

            return lista;
        }
        //Conexión a la BBDD.
        private static void ConectarBBDD()
        {
            var settings = MongoClientSettings.FromConnectionString("mongodb+srv://aprat:1234@cluster0.6rp8h.mongodb.net/myFirstDatabase?retryWrites=true&w=majority");
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            dbClient = new MongoClient(settings);
            db = dbClient.GetDatabase("cuenta");
        }
        public static void limpiar()
        {
            Console.WriteLine("Pulsa cualquier tecla...");
            Console.ReadLine();
            Console.Clear();
        }
    }
}
