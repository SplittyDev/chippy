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
		Emulator emulator;

		RichTextBox log;

		public MainForm () {
			InitializeComponents ();
			InitializeLog ();
			InitializeEmulator ();
		}

		void InitializeLog () {
			Console.SetOut (new ControlWriter (log));
		}

		void InitializeEmulator () {
			emulator = Emulator.Instance;
			emulator
				.Connect<ManagedMemory> ()
				.Connect<ManagedCPU> ()
				.Connect<WinFormsDisplay> ();
			this.Controls.Add ((emulator.Screen as WinFormsDisplay));
		}

		void InitializeComponents () {
			this.StartPosition = FormStartPosition.CenterScreen;
			this.FormBorderStyle = FormBorderStyle.FixedSingle;
			this.Width = 640;
			this.Height = 480;
			this.BackgroundImage = Image.FromFile ("default.png");
			this.BackgroundImageLayout = ImageLayout.Center;

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
			menu_emulator.MenuItems.Add ("Soft reset", delegate {
				emulator.SoftReset ();
			});
			menu_emulator.MenuItems.Add ("Hard reset (experimental)", delegate {
				emulator.HardReset ();
				InitializeEmulator ();
			});

			// Debug menu
			var menu_debug = new MenuItem ("Debug");
			menu_debug.MenuItems.Add ("Dump RAM", DumpRam);

			menu.MenuItems.AddRange (new [] { menu_program, menu_emulator, menu_debug });
			this.Menu = menu;
			#endregion

			// Log RichTextBox
			log = new RichTextBox {
				Height = 100,
				Dock = DockStyle.Bottom,
				Text = "",
				AutoWordSelection = false,
				DetectUrls = true,
				EnableAutoDragDrop = false,
				ScrollBars = RichTextBoxScrollBars.ForcedVertical,
				BorderStyle = BorderStyle.FixedSingle,
				Multiline = true,
			};
			this.Controls.Add (log);
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
				emulator.LoadFile (dialog.FileName);
				emulator.RunTask ();
			}
		}

		void DumpRam (object sender, EventArgs e) {
			var ram = emulator.DumpRom ();
			using (var fs = new FileStream ("dump.bin", FileMode.Create, FileAccess.Write, FileShare.None))
			using (var writer = new BinaryWriter (fs)) {
				writer.Write (ram);
			}
		}
	}
}

