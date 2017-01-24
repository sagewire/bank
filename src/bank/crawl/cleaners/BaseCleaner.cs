using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bank.crawl.cleaners
{
    interface ICleaner
    {
        Page Page { get; set; }

        void Clean();
    }

    class BaseCleaner : ICleaner
    {
        public Page Page { get; set; }

        public virtual void Clean()
        {

        }
    }
}
