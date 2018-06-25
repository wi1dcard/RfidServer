using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RfidServer.Exceptions
{
	class ReaderException : Exception
	{
		public ReaderException(string message) : base(message)
		{
		}
	}
}
