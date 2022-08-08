using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vxlapi_NET;

namespace VectorController.Processor
{
    internal class CanBus
    {

        CommonVector common = new(new XLDriver());

        public void test() 
        {
            common.OpenDriver();
            common.GetDriverConfig();
            common.GetDLLVesrion();
            common.GetListOfChannels();


        }




    }
}
