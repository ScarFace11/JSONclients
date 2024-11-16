using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace c_sharp_Json_reader
{
    class Program
    {
        static void Main(string[] args)
        {
            // Путь к JSON-файлу
            string filePath = "data.json";

            // Проверка, существует ли файл
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Файл не найден!");
                return;
            }         

            try
            {
                // Чтение содержимого файла
                string jsonContent = File.ReadAllText(filePath);
                JToken jsonData = JToken.Parse(jsonContent);

                // Если данные не в виде массива, но это объект, оборачиваем в массив
                if (!(jsonData is JArray))
                {
                    jsonData = new JArray(jsonData); // Оборачиваем в массив
                }


                // Применение проверки к каждому пользователю
                foreach (JObject user in jsonData)
                {
                    Console.WriteLine($"Проверка клиента {user["firstName"]} {user["lastName"]}");
                    bool isValid = CustomerVerification(user);                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при чтении или парсинге JSON: " + ex.Message);
            }

            bool CustomerVerification(JObject jsonData)
            {
                // Проверка наличия данных о дате рождения
                string birthDateStr = jsonData["birthDate"]?.ToString();
                if (string.IsNullOrEmpty(birthDateStr) || !DateTime.TryParse(birthDateStr, out DateTime birthDate))
                {
                    Console.Write("Неверный формат или отсутствие даты рождения - ");
                    PrintRed("Отказ!");
                    return false;
                }

                // Вычисление возраста
                int age = DateTime.Now.Year - birthDate.Year;
                if (DateTime.Now < birthDate.AddYears(age))
                    age--;

                // Проверка минимального возраста
                if (age < 20)
                {
                    Console.Write("Клиент моложе 20 лет - ");
                    PrintRed("Отказ!");
                    return false;
                }

                // Проверка наличия данных о дате выдачи паспорта
                string passportDateStr = jsonData["passport"]?["issuedAt"]?.ToString();
                if (string.IsNullOrEmpty(passportDateStr) || !DateTime.TryParse(passportDateStr, out DateTime passportDate))
                {
                    Console.Write("Неверный формат или отсутствие даты выдачи паспорта - ");
                    PrintRed("Отказ!");
                    return false;
                }

                // Проверка условий обновления паспорта
                if ((age > 20 && passportDate < birthDate.AddYears(20)) ||
                    (age > 45 && passportDate < birthDate.AddYears(45)))
                {
                    Console.Write("Требуется обновить паспорт - ");
                    PrintRed("Отказ!");
                    return false;
                }

                // Чтение кредитной истории
                JArray creditHistory = (JArray)jsonData["creditHistory"];
                return CreditHistoryCheck(creditHistory);
            }

            bool CreditHistoryCheck(JArray creditHistory)
            {
                if (creditHistory == null)
                {
                    Console.Write("Кредитная история отсутствует - ");
                    PrintRed("Отказ!");
                    return false;
                }

                // Счётчик кредитов с просроченной задолженностью более 15 дней
                int overdueCountMoreThan15Days = 0;

                foreach (JObject credit in creditHistory)
                {
                    // Извлечение данных о типе кредита и просроченной задолженности
                    string type = credit["type"]?.ToString();
                    double currentOverdueDebt = credit["currentOverdueDebt"]?.ToObject<double>() ?? 0;
                    int numberOfDaysOnOverdue = credit["numberOfDaysOnOverdue"]?.ToObject<int>() ?? 0;

                    // Проверка наличия непогашенной просроченной задолженности
                    if (currentOverdueDebt > 0)
                    {
                        Console.Write("Есть непогашенная задолжность - ");
                        PrintRed("Отказ!");
                        return false;
                    }

                    if (type != "Кредитная карта")
                    {
                        // Условия для кредита, который не является "Кредитной картой"
                        if (numberOfDaysOnOverdue > 60)
                        {
                            Console.Write("Срок задолженности больше 60 дней - ");
                            PrintRed("Отказ!");
                            return false;
                        }

                        if (numberOfDaysOnOverdue > 15)
                        {
                            overdueCountMoreThan15Days++;
                        }
                    }
                    else // Условия для кредита "Кредитная карта"
                    {
                        if (numberOfDaysOnOverdue > 30)
                        {
                            Console.Write("Срок задолженности больше 30 дней - ");
                            PrintRed("Отказ!");
                            return false;
                        }
                    }
                }

                // Проверка, если есть больше двух кредитов с просроченной задолженностью более 15 дней
                if (overdueCountMoreThan15Days > 2)
                {
                    Console.Write("2 и более просроченных кредита - ");
                    return false;
                }
                Console.Write("Проверка пройдена - ");
                PrintGreen("Принято!");
                return true; // Проверка пройдена
            }

            // Метод для вывода текста красным цветом
            static void PrintRed(string message)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(message);
                Console.ResetColor();
            }

            // Метод для вывода текста зелёным цветом
            static void PrintGreen(string message)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(message);
                Console.ResetColor();
            }
        }       
    }
}
