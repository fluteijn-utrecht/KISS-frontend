using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kiss.Bff.EndToEndTest.ContactMomentSearch.Helpers;

namespace Kiss.Bff.EndToEndTest.ContactMomentSearch
{
    [TestClass]
    public class ContactMomentSearchScenarios : KissPlaywrightTest
    {
        #region Test Cases

        // 1. Searching by Last Name and Date of Birth (Valid)
        [TestMethod]
        public async Task SearchByLastNameAndDOB_ValidAsync()
        {


        }

        // 2. Searching by Last Name and Date of Birth (Not Found)
        [TestMethod]
        public async Task SearchByLastNameAndDOB_NotFound()
        { 
            
        }

        #endregion
    }
}
