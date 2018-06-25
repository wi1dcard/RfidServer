using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RfidServer.Exceptions
{
	class HttpException : Exception
	{
		public HttpException(string message) : base(message)
		{
		}
	}
}
