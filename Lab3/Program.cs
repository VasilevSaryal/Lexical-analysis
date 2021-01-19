using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab3
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        static string[] operatorChars = { "=", ":=", "+" };
        static string[] specialChars = { ":", ";", ".", ",", "(", ")" };
        static string[] serviceWords = { "var", "const", "integer", "begin", "for", "to", "do", "end" };
        static List<String> identifiers = new List<String>();
        static List<String> numbers = new List<String>();
        static List<String> strings = new List<String>();

        public static List<Char> CharList = new List<Char>();
        static List<Char> Buf = new List<Char>();

        //выдает очередной символ компилируемой программы 
        //(очередной символ входного потока) 
        static char GetInputChar()
        {
            char c = CharList.FirstOrDefault();
            if (CharList.Count > 0)
            {
                CharList.RemoveAt(0);
                return c;
            }
            return '\ud801';
        }

        //возвращает символ с во входной поток
        static void RevLit(char c)
        {
            CharList.Insert(0, c);
        }

        //добавляет символ с в буфер Buf
       static void PutInBuf(char c)
        {
            Buf.Add(c);
        }

        //возвращает true, если c – цифра 0..9
        static bool Number(char c)
        {
            return Char.IsDigit(c);
        }

        //возвращает true, если c – лат.буква или _
        static bool Letter(char c)
        {
            return (Char.IsLetter(c) || c == '_');
        }

        // определить, является ли содержимое 
        //буфера Buf ключевым словом. 
        //Если нет, определить лежит ли Buf 
        //в таблице идентефикаторов. Если нет, 
        //то добавить. В любом случае в Type 
        // записать тип и вернуть номер. 
        static int Buf_Intend_Key(ref int Type)
        {
            Console.WriteLine("Buf_Intend_Key: ");
            Console.Write(new string(Buf.ToArray()));
            if (serviceWords.Contains(new string(Buf.ToArray())))
            {
                Type = 1;
                return Array.IndexOf(serviceWords, new string(Buf.ToArray()));
            }
            if (identifiers.Contains(new string(Buf.ToArray()))) {
                Console.WriteLine("identifiers");
                Type = 4;
                return Array.IndexOf(identifiers.ToArray(), new string(Buf.ToArray()));
            }
            identifiers.Add(new string(Buf.ToArray()));
            Console.WriteLine("new identifiers");
            Type = 4;
            return identifiers.Count - 1;
        }

        //найти содержимое буфера Buf в таблице
        //операций и вернуть тип и номер.
        static int Buf_Oper(ref int Type)
        {
            if (operatorChars.Contains(new string(Buf.ToArray())))
            {
                Type = 2;
                return Array.IndexOf(operatorChars, new string(Buf.ToArray()));
            }
            return -1;
        }

        //найти содержимое буфера Buf в таблице
        //спецсимволов и вернуть тип и номер.
        static int Buf_Spec(ref int Type)
        {
            if (specialChars.Contains(new string(Buf.ToArray())))
            {
                Type = 3;
                return Array.IndexOf(specialChars, new string(Buf.ToArray()));
            }
            return -1;
        }

        //добавить содержимое буфера Buf в таблицу
        //числовых констант и вернуть тип и номер.
        static int Buf_Num(ref int Type)
        {
            numbers.Add(new string(Buf.ToArray()));
            Type = 5;
            return numbers.Count - 1;
        }

        //добавить содержимое буфера Buf в таблицу
        //строковых констант и вернуть тип и номер.
        static int Buf_Str(ref int Type)
        {
            strings.Add(new string(Buf.ToArray()));
            Type = 6;
            return strings.Count - 1;
        }

        enum State { S, cm, id, ss, cf, st, dp, fin };

        //Один шаг лексического
        //анализатора
        //Возвращает:
        // -2 – конец потока.
        //-1 – ошибка
        // в остальных случаях –
        // номер распозанной лексемы
        // и в Type – ее тип
        private static bool quotesOne = true; //костыль два или одна ковычка в st
        static int StepLexAnalize(ref int Type)
        {
            char c; //очередная литера
            State T = State.S; //начальное значение очередного (текущего) состояния
            // Инвариант цикла
            // На вход автомата Alex подается
            // компилируемая программа.
            // Один оборот цикла соответствует
            // одном шагу автомата Alex. В начале оборота:
            // Значение переменной T – очередное состояние автомата.
            // Остаток входного потока – еще не считанная цепочка.
            do {
                c = GetInputChar();
                switch (T) {
                    case State.S:
                        if ((c == ' ') || (c =='\t') || (c =='\r') || (c =='\n'))
                            break;
                        if (c =='{')
                        { 
                            T = State.cm;
                            break; 
                        }
                        if (c =='\'' || c == '\"')
                        {
                            quotesOne = (c == '\'');
                            T = State.st;
                            break;
                        }
                        if (c == '\ud801')
                            return -2;
                        //Выше перечислены случаи, когда с
                        //не отправляется в буфер
                        PutInBuf(c);
                        if (Letter(c)) {
                            T = State.id;
                            break;
                        }
                        if ((c =='=')|| (c =='+'))
                            return Buf_Oper(ref Type);
                        if (c ==':') {
                            T = State.ss;
                            break;
                        }
                        if ((c =='.')|| (c ==';') || (c =='(')|| (c ==')') || (c == ',') || (c == ':'))
                                return Buf_Spec(ref Type);
                        if (Number(c))
                        {
                            T = State.cf;
                            break;
                        }
                        return -1; //Ошибка
                    case State.id:
                        if (Number(c) || Letter(c)) {
                            PutInBuf(c);
                            break;
                        } else
                        {
                            RevLit(c);
                            return Buf_Intend_Key(ref Type);
                        }
                    case State.ss:
                        if (c == '=')
                        {
                            PutInBuf(c);
                            return Buf_Oper(ref Type);
                        }
                        RevLit(c);
                        return Buf_Spec(ref Type);
                    case State.cf:
                        if (Number(c))
                        {
                            PutInBuf(c);
                            break;
                        }
                        RevLit(c);
                        return Buf_Num(ref Type);
                    case State.st:
                        if (c == '\ud801')
                            return -1;
                        if (c == '\'' && quotesOne || c == '\"' && !quotesOne)
                            T = State.dp;
                        else
                            PutInBuf(c);
                        break;
                    case State.dp:
                        if (c == '\'')
                        {
                            PutInBuf(c);
                            T = State.st;
                        }
                        RevLit(c);
                        return Buf_Str(ref Type);
                }
            } while (true);
        }

        //---------------------------
        //Функция StepLexAnalize() используется в следующем
        //основном модуле, которому требуется еще функция
        //void PutInOut(int N,int T); отправляюшая пару
        //<T,N> в результирующий поток лексем.
        public static void LexAnalize(ref int Type) //Лексический анализатор 
        {
            int N;
            do
            {
                N = StepLexAnalize(ref Type);
                if (N == -1)
                {
                    MessageBox.Show("", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (N == -2)
                {
                    MessageBox.Show("", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                PutInOut(N, ref Type);
                Buf.Clear();
                Type = 0;
            } while (true);
        }
        //-----------------------------


        // Найденные лексемы
        public static List<String> findedOperatorChars = new List<String>();
        public static List<String> findedSpecialChars = new List<String>();
        public static List<String> findedServiceWords = new List<String>();
        public static List<String> findedIdentifiers = new List<String>();
        public static List<String> findedNumbers = new List<String>();
        public static List<String> findedStrings = new List<String>();

        static void PutInOut(int N, ref int Type)
        {       
            switch (Type)
            {
                case 1:
                    if (!findedServiceWords.Contains(serviceWords[N]))
                        findedServiceWords.Add(serviceWords[N]);
                    break;
                case 2:
                    if (!findedOperatorChars.Contains(operatorChars[N]))
                        findedOperatorChars.Add(operatorChars[N]);
                    break;
                case 3:
                    if (!findedSpecialChars.Contains(specialChars[N]))
                        findedSpecialChars.Add(specialChars[N]);
                    break;
                case 4:
                    if (!findedIdentifiers.Contains(identifiers[N]))
                        findedIdentifiers.Add(identifiers[N]);
                    break;
                case 5:
                    if (!findedNumbers.Contains(numbers[N]))
                        findedNumbers.Add(numbers[N]);
                    break;
                case 6:
                    if (!findedStrings.Contains(strings[N]))
                        findedStrings.Add(strings[N]);
                    break;
            }
        }

        public static void ClearAll()
        {
            findedOperatorChars.Clear();
            findedSpecialChars.Clear();
            findedServiceWords.Clear();
            findedIdentifiers.Clear();
            findedNumbers.Clear();
            findedStrings.Clear();

            Buf.Clear();
            CharList.Clear();

            strings.Clear();
            numbers.Clear();
            identifiers.Clear();
        }
    }
}
