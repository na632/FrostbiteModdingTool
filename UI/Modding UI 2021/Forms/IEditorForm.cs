using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modding_UI_2021.Forms
{
    public interface IEditorForm
    {
        void OpenTheGame();
        Task<bool> PreStart();

        Task<bool> Start();


        Task<bool> PostStart();

    }
}
