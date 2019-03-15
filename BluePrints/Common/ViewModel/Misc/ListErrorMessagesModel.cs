using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.ViewModel.Misc
{
    public class ErrorMessage
    {
        public ErrorMessage(string name, string error)
        {
            NAME = name;
            ERROR = error;
        }

        public string NAME { get; set; }
        public string ERROR { get; set; }
    }
}
