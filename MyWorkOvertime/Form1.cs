using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IDLL;
using DLL;
using Model;
using Model.Filter;

namespace MyWorkOvertime
{
    public partial class Form1 : Form
    {
        IWorkRecService sevice = new WorkRecService();
        public Form1()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            var request = new BaseRequest<WorkRecFilter>()
            {
                CurrentPage = 1,
                PageSize = 10,
            };

           
            sevice.QueryRec();

        }
    }
}
