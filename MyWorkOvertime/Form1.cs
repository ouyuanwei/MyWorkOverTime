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
using Model.Model;

namespace MyWorkOvertime
{
    public partial class Form1 : Form
    {
        int nowPage = 0;
        int allPage = 0;
        IWorkRecService sevice = new WorkRecService();
        public Form1()
        {
            InitializeComponent();

            var nowDate = DateTime.Now;
            dateTimePicker1.Text = nowDate.Year + "-" + (nowDate.Month-1) + "-" + nowDate.Day;
            dateTimePicker2.Text = nowDate.Year + "-" + nowDate.Month+ "-" + nowDate.Day;
            GetWorkRec(1);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            nowPage = 1;
            GetWorkRec(nowPage);
        }

        private void btnFirst_Click(object sender, EventArgs e)
        {
            GetWorkRec(1);
        }

        private void btnUp_Click(object sender, EventArgs e)
        {

            if (!int.TryParse(txtNowPage.Text, out nowPage))
            {
                MessageBox.Show("操作失败");
                return;
            }
            if (nowPage > 1)
            {
                nowPage--;
                GetWorkRec(nowPage);
            }
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtNowPage.Text, out nowPage))
            {
                MessageBox.Show("操作失败");
                return;
            }
            GetWorkRec(nowPage);
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtAllPage.Text, out allPage))
            {
                MessageBox.Show("操作失败");
                return;
            }
            if (allPage > nowPage)
            {
                nowPage++;
                GetWorkRec(nowPage);
            }
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtAllPage.Text, out allPage))
            {
                MessageBox.Show("操作失败");
                return;
            }
                GetWorkRec(allPage);
        }

        private void GetWorkRec(int nowPage)
        {

            var request = new BaseRequest<WorkRecFilter>()
            {
                CurrentPage = nowPage,
                PageSize = 10,
                Data = {
                     EndTime=string.IsNullOrEmpty( dateTimePicker2.Text)?DateTime.Now.AddDays(1): DateTime.Parse(dateTimePicker2.Text).AddDays(1),
                      StartTime=string.IsNullOrEmpty( dateTimePicker1.Text)?DateTime.Now.AddMonths(-1):DateTime.Parse(dateTimePicker1.Text),
                }

            };
            var resquest = sevice.QueryRec(request);

            if (resquest.ResultCode >= 0)
            {
                dataGridView1.DataSource = resquest.Data.RecList;
                richTextBox1.Text = "总计:" + resquest.Data.AllHour + ";    剩余:" + resquest.Data.EffectiveHour + ";    使用:" + resquest.Data.UseHour
                    + ";    过期:" + resquest.Data.OverdueHour;
                txtNowPage.Text = nowPage.ToString();
                txtAllPage.Text = resquest.PagesCount.ToString();
            }
            else
            { MessageBox.Show(resquest.ResultMessage); }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form1.ActiveForm.Height = Form1.ActiveForm.Height == 580 ? 760 : 580;
            button2.Text = Form1.ActiveForm.Height == 760 ? "隐藏" : "显示";
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var resquest = sevice.SaveRec(new Model.Model.WorkRecModel()
            {
                IsDelete = false,
                IsUse = false,
                Rmark = richTextBox2.Text,
                EndTime = DateTime.Parse(dateTimePicker4.Text),
                StartTime = DateTime.Parse(dateTimePicker3.Text),
                Hour = int.Parse(textBox1.Text)
            });
            if(resquest.ResultCode>=0)
            {
                nowPage = 1;
                GetWorkRec(nowPage);
            }
            else
            {
                MessageBox.Show("操作失败");
            }
        }
        bool IsSave = true;

        private void dateTimePicker3_ValueChanged(object sender, EventArgs e)
        {
            if(!string.IsNullOrEmpty(dateTimePicker3.Text)&& !string.IsNullOrEmpty(dateTimePicker4.Text))
            {
                textBox1.Text = (DateTime.Parse(dateTimePicker4.Text).Hour - DateTime.Parse(dateTimePicker3.Text).Hour).ToString();
            }
        }

        private void dateTimePicker4_ValueChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(dateTimePicker3.Text) && !string.IsNullOrEmpty(dateTimePicker4.Text))
            {
                textBox1.Text = (DateTime.Parse(dateTimePicker4.Text).Hour - DateTime.Parse(dateTimePicker3.Text).Hour).ToString();
            }
        }

        private void dataGridView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var cells = dataGridView1.SelectedRows[0].Cells ;
            var resquest = sevice.SaveRec(new Model.Model.WorkRecModel()
            {
                IsDelete = (bool)cells["IsDelete"].Value,
                IsUse =true,
                Rmark = cells["Rmark"].Value.ToString(),
                EndTime =(DateTime) cells["EndTime"].Value,
                StartTime = (DateTime)cells["StartTime"].Value,
                Hour = (int)cells["Hour"].Value,
                Id= (int)cells["Id"].Value,
            });
            if (resquest.ResultCode >= 0)
            {
                nowPage = 1;
                GetWorkRec(nowPage);
            }
            else
            {
                MessageBox.Show("操作失败");
            }
        }
    }
}
