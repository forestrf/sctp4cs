using System;

static class Log {
	[System.Diagnostics.Conditional("DEBUG")]
	public static void debug(string txt) {
		Console.WriteLine(txt);
	}

	[System.Diagnostics.Conditional("DEBUG")]
	public static void verb(string txt) {
		Console.WriteLine(txt);
	}

	[System.Diagnostics.Conditional("DEBUG")]
	public static void warn(string txt) {
		Console.WriteLine(txt);
	}

	[System.Diagnostics.Conditional("DEBUG")]
	public static void error(string txt) {
		Console.WriteLine(txt);
	}
}

static class logger {
	[System.Diagnostics.Conditional("DEBUG")]
	public static void debug(string txt) {
		Console.WriteLine(txt);
	}

	[System.Diagnostics.Conditional("DEBUG")]
	public static void verb(string txt) {
		Console.WriteLine(txt);
	}

	[System.Diagnostics.Conditional("DEBUG")]
	public static void warn(string txt) {
		Console.WriteLine(txt);
	}

	[System.Diagnostics.Conditional("DEBUG")]
	public static void warn(string txt, Exception ex = null) {
		log(txt, ex);
	}

	[System.Diagnostics.Conditional("DEBUG")]
	public static void error(string txt) {
		Console.WriteLine(txt);
	}

	[System.Diagnostics.Conditional("DEBUG")]
	public static void severe(string txt) {
		Console.WriteLine(txt);
	}

	[System.Diagnostics.Conditional("DEBUG")]
	public static void severe(string txt, Exception ex = null) {
		log(txt, ex);
	}

	[System.Diagnostics.Conditional("DEBUG")]
	public static void info(string txt, Exception ex) {
		log(txt, ex);
	}

	[System.Diagnostics.Conditional("DEBUG")]
	public static void info(string txt) {
		Console.WriteLine(txt);
	}

	[System.Diagnostics.Conditional("DEBUG")]
	public static void fine(string txt) {
		Console.WriteLine(txt);
	}

	[System.Diagnostics.Conditional("DEBUG")]
	public static void fine(string txt, Exception ex) {
		log(txt, ex);
	}

	[System.Diagnostics.Conditional("DEBUG")]
	public static void finest(string txt) {
		Console.WriteLine(txt);
	}

	[System.Diagnostics.Conditional("DEBUG")]
	public static void finest(string txt, Exception ex) {
		log(txt, ex);
	}

	[System.Diagnostics.Conditional("DEBUG")]
	public static void finer(string txt) {
		Console.WriteLine(txt);
	}

	[System.Diagnostics.Conditional("DEBUG")]
	public static void finer(string txt, Exception ex) {
		log(txt, ex);
	}

	[System.Diagnostics.Conditional("DEBUG")]
	public static void warning(string txt) {
		Console.WriteLine(txt);
	}

	[System.Diagnostics.Conditional("DEBUG")]
	public static void log(string txt, Exception ex = null) {
		Console.WriteLine(txt);
		if (ex != null)
			Console.WriteLine(ex);
	}
}
