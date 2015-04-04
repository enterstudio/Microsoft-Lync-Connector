using System;
using System.Net;
using System.Net.Http;

namespace Telligent.Evolution.Extensions.Lync
{
	public enum LyncExceptionsType 
	{
		HttpError = 1,
		AutoDiscoveryNotFound = 2,
		GrantTypeNotSupported = 2,
		Unexpected = 999
	}

	public class LyncExceptions : Exception
	{
	    private readonly HttpResponseMessage response;
	    public LyncExceptionsType Type { get; private set; }

		public LyncExceptions(LyncExceptionsType type)
		{
			Type = type;
		}

		public LyncExceptions(LyncExceptionsType type, string message) 
			:base(message)
		{
			Type = type;
		}

		public LyncExceptions(LyncExceptionsType type, string message, Exception innerException)
			: base(message, innerException)
		{
			Type = type;
		}

		public LyncExceptions(LyncExceptionsType type, HttpResponseMessage response)
		{
		    this.response = response;
		    Type = type;
		}
	}

	public class LyncHttpExceptions : LyncExceptions
	{
		public Uri Url { get; private set; }

		public HttpStatusCode StatusCode { get; private set; }

		public HttpResponseMessage Response { get; private set; }

		public LyncHttpExceptions(HttpResponseMessage response)
			: base(LyncExceptionsType.HttpError)
		{
			Url = response.RequestMessage.RequestUri;
			StatusCode = response.StatusCode;
			Response = response;
		}
	}
}
