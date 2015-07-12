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
		Debugger debugger;

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
			debugger = Debugger.Instance;
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
			this.BackgroundImage = Image.FromFile ("default.png");
			this.BackgroundImageLayout = ImageLayout.Zoom;

			#region Menu
			var menu = new MainMenu ();

			// Program menu
			var menu_program = new MenuItem ("Program");
			menu_program.MenuItems.Add ("Load program", LoadProgram);
			menu_program.MenuItems.Add ("Exit", delegate {
				Application.Exit ();
			});

			// Emulator menu
			var menu_emulator = new MenuItem ("Emulator");

			// Debug menu
			var menu_debug = new MenuItem ("Debugger");
			menu_debug.MenuItems.Add (new MenuItem ("Start", (sender, e) => Debugger.Instance.Continue (), Shortcut.AltUpArrow));
			menu_debug.MenuItems.Add (new MenuItem ("Step", (sender, e) => Debugger.Instance.Step (), Shortcut.AltRightArrow));
			menu_debug.MenuItems.Add (new MenuItem ("Pause", (sender, e) => Debugger.Instance.Pause (), Shortcut.AltDownArrow));
			menu_debug.MenuItems.Add (new MenuItem ("Stop", (sender, e) => Debugger.Instance.Stop (), Shortcut.AltLeftArrow));
			menu_debug.MenuItems.Add ("Restart", (sender, e) => Debugger.Instance.Restart ());
			menu_debug.MenuItems.Add ("");
			menu_debug.MenuItems.Add (new MenuItem ("Dump RAM", DumpRam, Shortcut.CtrlD));
			menu_debug.MenuItems.Add (new MenuItem ("Dump ROM", DumpRom, Shortcut.CtrlShiftD));

			menu.MenuItems.AddRange (new [] { menu_program, menu_emulator, menu_debug });
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
			var ram = Emulator.Instance.DumpRam ();
			using (var fs = new FileStream ("ramdump.bin", FileMode.Create, FileAccess.Write, FileShare.None))
			using (var writer = new BinaryWriter (fs)) {
				writer.Write (ram);
			}
		}

		void DumpRom (object sender, EventArgs e) {
			var rom = Emulator.Instance.DumpRom ();
			using (var fs = new FileStream ("romdump.bin", FileMode.Create, FileAccess.Write, FileShare.None))
			using (var writer = new BinaryWriter (fs)) {
				writer.Write (rom);
			}
		}
	}
}

