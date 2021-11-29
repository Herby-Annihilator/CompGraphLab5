using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lab5
{
	public class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			var app = new App();
			app.InitializeComponent();
			app.Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
		   Host.CreateDefaultBuilder(args)
			  .ConfigureServices(App.ConfigureServices)
	   ;
	}
}
