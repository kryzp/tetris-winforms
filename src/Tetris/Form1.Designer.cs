namespace Tetris
{
	partial class Form1
	{
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			gameTickTimer = new System.Windows.Forms.Timer(components);
			label1 = new Label();
			label2 = new Label();
			label3 = new Label();
			label4 = new Label();
			label5 = new Label();
			label6 = new Label();
			resetButton = new Button();
			label7 = new Label();
			backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
			scoreLabel = new Label();
			upNextLabel = new Label();
			highscoreLabel = new Label();
			SuspendLayout();
			// 
			// label1
			// 
			label1.Font = new Font("Segoe UI Black", 18F, FontStyle.Bold, GraphicsUnit.Point);
			label1.Location = new Point(499, 272);
			label1.Name = "label1";
			label1.Size = new Size(201, 82);
			label1.TabIndex = 0;
			label1.Text = "Tetris";
			label1.TextAlign = ContentAlignment.MiddleCenter;
			// 
			// label2
			// 
			label2.Location = new Point(499, 360);
			label2.Name = "label2";
			label2.Size = new Size(201, 64);
			label2.TabIndex = 1;
			label2.Text = "[LARROW] to move left";
			// 
			// label3
			// 
			label3.Location = new Point(499, 439);
			label3.Name = "label3";
			label3.Size = new Size(201, 64);
			label3.TabIndex = 2;
			label3.Text = "[RARROW] to move right";
			// 
			// label4
			// 
			label4.Location = new Point(499, 519);
			label4.Name = "label4";
			label4.Size = new Size(201, 45);
			label4.TabIndex = 3;
			label4.Text = "[X] to hit bottom";
			// 
			// label5
			// 
			label5.Location = new Point(499, 564);
			label5.Name = "label5";
			label5.Size = new Size(201, 52);
			label5.TabIndex = 4;
			label5.Text = "[Z] to rotate";
			// 
			// label6
			// 
			label6.Location = new Point(499, 607);
			label6.Name = "label6";
			label6.Size = new Size(201, 80);
			label6.TabIndex = 5;
			label6.Text = "[C] to store for later";
			// 
			// resetButton
			// 
			resetButton.Location = new Point(499, 901);
			resetButton.Name = "resetButton";
			resetButton.Size = new Size(201, 68);
			resetButton.TabIndex = 6;
			resetButton.Text = "Reset";
			resetButton.UseVisualStyleBackColor = true;
			resetButton.Click += resetButton_Click;
			// 
			// label7
			// 
			label7.AutoSize = true;
			label7.Location = new Point(499, 702);
			label7.Name = "label7";
			label7.Size = new Size(78, 32);
			label7.TabIndex = 7;
			label7.Text = "Score:";
			// 
			// scoreLabel
			// 
			scoreLabel.Font = new Font("Segoe UI", 19.875F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point);
			scoreLabel.Location = new Point(510, 752);
			scoreLabel.Name = "scoreLabel";
			scoreLabel.Size = new Size(190, 92);
			scoreLabel.TabIndex = 8;
			scoreLabel.TextAlign = ContentAlignment.MiddleCenter;
			// 
			// upNextLabel
			// 
			upNextLabel.Location = new Point(499, 212);
			upNextLabel.Name = "upNextLabel";
			upNextLabel.Size = new Size(201, 72);
			upNextLabel.TabIndex = 9;
			upNextLabel.Text = "Up Next:";
			upNextLabel.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// highscoreLabel
			// 
			highscoreLabel.AutoSize = true;
			highscoreLabel.Location = new Point(499, 856);
			highscoreLabel.Name = "highscoreLabel";
			highscoreLabel.Size = new Size(126, 32);
			highscoreLabel.TabIndex = 10;
			highscoreLabel.Text = "Highscore:";
			// 
			// Form1
			// 
			AutoScaleDimensions = new SizeF(13F, 32F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(1244, 1060);
			Controls.Add(highscoreLabel);
			Controls.Add(upNextLabel);
			Controls.Add(scoreLabel);
			Controls.Add(label7);
			Controls.Add(resetButton);
			Controls.Add(label6);
			Controls.Add(label5);
			Controls.Add(label4);
			Controls.Add(label3);
			Controls.Add(label2);
			Controls.Add(label1);
			Name = "Form1";
			Text = "Tetris";
			Load += Form1_Load;
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private System.Windows.Forms.Timer gameTickTimer;
		private Label label1;
		private Label label2;
		private Label label3;
		private Label label4;
		private Label label5;
		private Label label6;
		private Button resetButton;
		private Label label7;
		private System.ComponentModel.BackgroundWorker backgroundWorker1;
		private Label scoreLabel;
		private Label upNextLabel;
		private Label highscoreLabel;
	}
}