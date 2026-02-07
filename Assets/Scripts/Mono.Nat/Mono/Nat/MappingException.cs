using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Mono.Nat
{
	[Serializable]
	public class MappingException : Exception
	{
		private int errorCode;

		private string errorText;

		public int ErrorCode
		{
			get
			{
				return errorCode;
			}
		}

		public string ErrorText
		{
			get
			{
				return errorText;
			}
		}

		public MappingException()
		{
		}

		public MappingException(string message)
			: base(message)
		{
		}

		public MappingException(int errorCode, string errorText)
			: base(string.Format("Error {0}: {1}", errorCode, errorText))
		{
			this.errorCode = errorCode;
			this.errorText = errorText;
		}

		public MappingException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected MappingException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			errorCode = info.GetInt32("errorCode");
			errorText = info.GetString("errorText");
			base.GetObjectData(info, context);
		}
	}
}
