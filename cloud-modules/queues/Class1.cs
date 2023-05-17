using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace queues;
public class Class1
{

	public async Task<string> send(string msg)
	{
		Action<object> action = (object obj) => {
			Console.WriteLine("Hola {0}", obj);
		};

		await Task.Factory.StartNew(action, "beta");
		return "A message Identifier";
	}


	public async Task<string> receive(Action<string> onReceive)
	{
		string body = "this is the body";
		await  Task.Run(() => onReceive.Invoke(body));

		return "A message Identifier";
	}
}
