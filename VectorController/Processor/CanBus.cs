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

        CommonVector CommonVector { get; set; }


        public void test() 
        {
            CommonVector.GetDLLVesrion();
        }




    }
}
