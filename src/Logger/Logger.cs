using System.Diagnostics;

namespace SCTP4CS {
	public static class Logger {
		private const string KEYWORD = "SCHEDULER_";
		public static ILogger logger = new InternalLogger();

		[Conditional(KEYWORD + "TRACEVERBOSE"), Conditional(KEYWORD + "TRACE"), Conditional(KEYWORD + "DEBUG"), Conditional(KEYWORD + "INFO"), Conditional(KEYWORD + "WARN"), Conditional(KEYWORD + "ERROR")]
		public static void Error(string message) {
			logger.Error(message);
		}

		[Conditional(KEYWORD + "TRACEVERBOSE"), Conditional(KEYWORD + "TRACE"), Conditional(KEYWORD + "DEBUG"), Conditional(KEYWORD + "INFO"), Conditional(KEYWORD + "WARN")]
		public static void Warn(string message) {
			logger.Warn(message);
		}

		[Conditional(KEYWORD + "TRACEVERBOSE"), Conditional(KEYWORD + "TRACE"), Conditional(KEYWORD + "DEBUG"), Conditional(KEYWORD + "INFO")]
		public static void Info(string message) {
			logger.Info(message);
		}

		[Conditional(KEYWORD + "TRACEVERBOSE"), Conditional(KEYWORD + "TRACE"), Conditional(KEYWORD + "DEBUG")]
		public static void Debug(string message) {
			logger.Debug(message);
		}

		[Conditional(KEYWORD + "TRACEVERBOSE"), Conditional(KEYWORD + "TRACE")]
		public static void Trace(string message) {
			logger.Trace(message);
		}

		[Conditional(KEYWORD + "TRACEVERBOSE")]
		public static void TraceVerbose(string message) {
			logger.TraceVerbose(message);
		}
	}
}
