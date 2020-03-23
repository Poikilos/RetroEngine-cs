using System;
using System.Windows.Forms;
using System.ComponentModel; //for IContainer

namespace ExpertMultimedia {
	public class FastPanel : System.Windows.Forms.Panel {
		public FastPanel() {
			InitializeComponent();
			
			SetStyle(ControlStyles.UserPaint |
				ControlStyles.AllPaintingInWmPaint |
				ControlStyles.DoubleBuffer, true);
			
			SetStyle(ControlStyles.ResizeRedraw, true);
			//SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			SetStyle(ControlStyles.DoubleBuffer, true);
			UpdateStyles();
		}
		
		public FastPanel(IContainer container) {
			container.Add(this);
			
			InitializeComponent();
		}

			//oops this is designer-generated code -- let's panic, Bill Gates:
		private void InitializeComponent() {
			this.SuspendLayout();
			// 
			// GradientPanel
			// 
			this.ResumeLayout(false);
		}

	}//end FastPanel
}//end namespace
