using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using chippy8;

namespace chippy8gui
{
	public class MainForm : Form
	{
		public MainForm () {
			InitializeComponents ();
			InitializeLog ();
		}

		protected override void OnHandleCreated (EventArgs e)
		{
			InitializeEmulator ();
			base.OnHandleCreated (e);
		}

		void InitializeLog () {
			//Console.SetOut (new ControlWriter (log));
		}

		void InitializeEmulator () {
			Emulator.Instance
				.Connect<ManagedMemory> ()
				.Connect<ManagedCPU> ()
				.Connect<WinFormsDisplay> ();
			var disp = (Emulator.Instance.Screen as WinFormsDisplay);
			disp.AttachTo (this);
			disp.Width = 640;
			disp.Height = 320;
			disp.Location = new Point (0, 0);
		}

		void InitializeComponents () {
			this.StartPosition = FormStartPosition.CenterScreen;
			this.FormBorderStyle = FormBorderStyle.FixedSingle;
			this.Width = 640;
			this.Height = 480;
			this.BackColor = Color.FromArgb (230, 230, 230);
			this.BackgroundImageLayout = ImageLayout.Zoom;

			#region Menu
			var menu = new MainMenu ();

			// Program menu
			var menu_program = new MenuItem ("Program");
			menu_program.MenuItems.Add (new MenuItem ("Load program", LoadProgram, Shortcut.CtrlO));
			menu_program.MenuItems.Add (new MenuItem ("Exit", (sender, e) => Application.Exit (), Shortcut.CtrlX));

			// Emulator menu
			var menu_emulator = new MenuItem ("Emulator");
			menu_emulator.MenuItems.Add (new MenuItem ("Start", (sender, e) => Debugger.Instance.Continue (), Shortcut.AltUpArrow));
			menu_emulator.MenuItems.Add (new MenuItem ("Pause", (sender, e) => Debugger.Instance.Pause (), Shortcut.AltDownArrow));
			menu_emulator.MenuItems.Add (new MenuItem ("Stop", (sender, e) => Debugger.Instance.Stop (), Shortcut.AltLeftArrow));
			menu_emulator.MenuItems.Add ("Restart", (sender, e) => Debugger.Instance.Restart ());

			// Debug menu
			var menu_debug = new MenuItem ("Debugger");
			menu_debug.MenuItems.Add (new MenuItem ("Step", (sender, e) => Debugger.Instance.Step (), Shortcut.AltRightArrow));
			menu_debug.MenuItems.Add (new MenuItem ("Dump RAM", DumpRam, Shortcut.CtrlD));
			menu_debug.MenuItems.Add (new MenuItem ("Dump ROM", DumpRom, Shortcut.CtrlShiftD));

			// Views menu
			var menu_views = new MenuItem ("Views");
			menu_views.MenuItems.Add (new MenuItem ("Registers", ShowRegisters, Shortcut.CtrlShiftR));

			menu.MenuItems.AddRange (new [] { menu_program, menu_emulator, menu_debug, menu_views });
			this.Menu = menu;
			#endregion

			this.ClientSize = new Size (640, 320);
		}

		void LoadProgram (object sender, EventArgs e) {
			var dialog = new OpenFileDialog {
				Multiselect = false,
				ShowReadOnly = true,
				DefaultExt = "ch8",
				AutoUpgradeEnabled = true,
				CheckFileExists = true,
				CheckPathExists = true,
				DereferenceLinks = true,
				Filter = "CHIP8 Program|*.ch8;*.bin|All file types|*.*"
			};
			var result = dialog.ShowDialog ();
			if (result == DialogResult.OK) {
				Debugger.Instance.Stop ();
				Emulator.Instance.LoadFile (dialog.FileName);
				Debugger.Instance.Run ();
			}
		}

		void DumpRam (object sender, EventArgs e) {
			var path = Path.Combine (Application.StartupPath, "ramdump.bin");
			var ram = Emulator.Instance.DumpRam ();
			using (var fs = new FileStream (path, FileMode.Create, FileAccess.Write, FileShare.None))
			using (var writer = new BinaryWriter (fs)) {
				writer.Write (ram);
			}
		}

		void DumpRom (object sender, EventArgs e) {
			var path = Path.Combine (Application.StartupPath, "romdump.bin");
			var rom = Emulator.Instance.DumpRom ();
			using (var fs = new FileStream (path, FileMode.Create, FileAccess.Write, FileShare.None))
			using (var writer = new BinaryWriter (fs)) {
				writer.Write (rom);
			}
		}

		bool registers_shown;
		Label registers_lblreg;
		void ShowRegisters (object sender, EventArgs e) {
			if (registers_lblreg == null) {
				registers_lblreg = new Label {
					AutoSize = false,
					Dock = DockStyle.Right,
					Width = 298,
					Font = new Font (FontFamily.GenericMonospace, 11.25f)
				};
			}
			if (!registers_shown) {
				this.Controls.Add (registers_lblreg);
				UpdateRegisters (update: true);
				this.Width += 300;
				registers_lblreg.Refresh ();
				registers_shown = true;
			} else {
				this.Width -= 300;
				UpdateRegisters (update: false);
				this.Controls.Remove (registers_lblreg);
				registers_shown = false;
			}
		}

		bool update_registers;
		void UpdateRegisters (bool update = true) {
			if (update == false) {
				update_registers = false;
				return;
			}
			Task.Factory.StartNew (() => {
				update_registers = true;
				while (update_registers) {
					Thread.Sleep (50);
					if (registers_lblreg == null)
						continue;
					var snap = Emulator.Instance.Cpu.Snapshot ();
					this.Invoke (new MethodInvoker (() => {
						registers_lblreg.Text = string.Format (
							"V0:    0x{0:X4} V8:    0x{8:X4}\n" +
							"V1:    0x{1:X4} V9:    0x{9:X4}\n" +
							"V2:    0x{2:X4} VA:    0x{10:X4}\n" +
							"V3:    0x{3:X4} VB:    0x{11:X4}\n" +
							"V4:    0x{4:X4} VC:    0x{12:X4}\n" +
							"V5:    0x{5:X4} VD:    0x{13:X4}\n" +
							"V6:    0x{6:X4} VE:    0x{14:X4}\n" +
							"V7:    0x{7:X4} Carry: 0x{15:X4}\n" +
							"Index: 0x{16:X4}\n" +
							"PC:    0x{17:X4}\n" +
							"SP:    0x{18:X4}",
							snap.V0, snap.V1, snap.V2, snap.V3, snap.V4,
							snap.V5, snap.V6, snap.V7, snap.V8, snap.V9,
							snap.VA, snap.VB, snap.VC, snap.VD, snap.VE,
							snap.Carry, snap.I, snap.PC, snap.SP
						);
						registers_lblreg.Update ();
					}));
				}
			});
		}
	}
}

