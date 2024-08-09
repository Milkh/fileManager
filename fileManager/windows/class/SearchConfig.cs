using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fileManager //.internal class
{
    public class SearchConfig
    {
        // Свойство для хранения стартовой директории
        public string StartDirectory { get; set; }
        // Свойство для хранения шаблона регулярного выражения
        public string RegexPattern { get; set; }
        public string RootDirectory { get; set; }
    }
}
