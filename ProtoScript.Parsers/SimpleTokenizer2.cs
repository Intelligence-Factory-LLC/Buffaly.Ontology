using System.Runtime.CompilerServices;

namespace ProtoScript.Parsers.BasicUtilities
{
	/// <summary>
	/// Allocation-minimal tokenizer.
	/// Updated to align with specific behaviors of an original Tokenizer version.
	/// </summary>
	public class SimpleTokenizer2
	{
		// fast ASCII tables (no hashing)
		private readonly bool[] m_arrQuotes = new bool[char.MaxValue + 1];
		private readonly bool[] m_arrSymbols = new bool[char.MaxValue + 1];

		// text & position
		protected string m_strTarget = string.Empty;  // keep for IndexOf in moveTo/movePast if using CurrentCulture
		protected ReadOnlyMemory<char> m_memTarget;
		protected int m_szCursor;

		// ───────────────────────────────────────────────────────── symbols / quotes
		public void insertSymbol(char c) { m_arrSymbols[c] = true; }
		public void removeSymbol(char c) { m_arrSymbols[c] = false; }
		public void clearSymbols() { Array.Clear(m_arrSymbols, 0, m_arrSymbols.Length); }

		public void insertQuote(char c) { m_arrQuotes[c] = true; }
		public void removeQuote(char c) { m_arrQuotes[c] = false; }
		public void clearQuotes() { Array.Clear(m_arrQuotes, 0, m_arrQuotes.Length); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public bool isQuote(char c) => m_arrQuotes[c];
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public bool isSymbol(char c) => m_arrSymbols[c];
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public bool isSpace(char c) => char.IsWhiteSpace(c);

		// ───────────────────────────────────────────────────────── ctor / reset
		public SimpleTokenizer2(string text)
		{
			setString(text);
			// Default quotes from original
			insertQuote('"');
			insertQuote('\'');
			insertQuote('`');
		}

		public void setString(string text)
		{
			m_strTarget = text ?? string.Empty; // Robustness: original might throw NRE later if text is null
			m_memTarget = m_strTarget.AsMemory();
			m_szCursor = 0;
		}

		// ───────────────────────────────────────────────────────── cursor helpers
		public string getString() => m_strTarget;
		public int getCursor() => m_szCursor;

		public void setCursor(int pos)
		{
			// Clamping to valid range [0, Length]
			if (pos < 0) m_szCursor = 0;
			else if (pos > m_memTarget.Length) m_szCursor = m_memTarget.Length;
			else m_szCursor = pos;
		}

		public bool hasMoreChars() => m_szCursor < m_memTarget.Length;

		/// <summary>
		/// Checks if there are more tokens available by scanning ahead.
		/// A character starts a token if it's a symbol, a quote, or not whitespace.
		/// Whitespace characters that are also symbols or quotes are considered tokens.
		/// </summary>
		public bool hasMoreTokens()
		{
			ReadOnlySpan<char> s = m_memTarget.Span;
			int len = s.Length;
			for (int i = m_szCursor; i < len; i++)
			{
				char c = s[i];
				// If char is a symbol, it's a token.
				// If char is a quote, it's a token.
				// If char is not whitespace, it's a token.
				// Otherwise (char is whitespace AND not a symbol AND not a quote), skip it.
				if (isSymbol(c) || isQuote(c) || !isSpace(c))
				{
					return true; // Found a character that will start/be a token
				}
			}
			return false; // No characters found that would start a new token
		}


		// ───────────────────────────────────────────────────────── char helpers
		public char getNextChar() => m_szCursor < m_memTarget.Length ? m_memTarget.Span[m_szCursor++] : '\0';

		public void discardNextChar()
		{
			if (m_szCursor < m_memTarget.Length)
			{
				m_szCursor++;
			}
		}

		public void discardChars(int cnt)
		{
			if (cnt <= 0) return;
			m_szCursor = Math.Min(m_szCursor + cnt, m_memTarget.Length);
		}

		public char peekNextChar() => m_szCursor < m_memTarget.Length ? m_memTarget.Span[m_szCursor] : '\0';

		public char peekChar(int offset)
		{
			int pos = m_szCursor + offset;
			if (pos >= 0 && pos < m_memTarget.Length) // Ensure pos is within bounds
				return m_memTarget.Span[pos];
			return '\0';
		}

		// ───────────────────────────────────────────────────────── delimiters
		public void moveTo(string delim)
		{
			if (string.IsNullOrEmpty(delim) || m_szCursor >= m_strTarget.Length) return;
			// Using m_strTarget for IndexOf to allow StringComparison.Ordinal
			int idx = m_strTarget.IndexOf(delim, m_szCursor, StringComparison.Ordinal);
			m_szCursor = idx != -1 ? idx : m_memTarget.Length;
		}

		public void movePast(string delim)
		{
			if (string.IsNullOrEmpty(delim) || m_szCursor >= m_strTarget.Length) return;
			// Using m_strTarget for IndexOf to allow StringComparison.Ordinal
			int idx = m_strTarget.IndexOf(delim, m_szCursor, StringComparison.Ordinal);
			if (idx == -1)
			{
				m_szCursor = m_memTarget.Length;
				return;
			}
			m_szCursor = Math.Min(idx + delim.Length, m_memTarget.Length);
		}

		/// <summary>
		/// Gets the substring from the current cursor position up to the specified delimiter.
		/// The cursor is moved to the position of the delimiter.
		/// If the delimiter is not found, returns the rest of the string and moves cursor to the end.
		/// This matches original Tokenizer's getTokenTo behavior.
		/// </summary>
		public string getTokenTo(string delim)
		{
			if (string.IsNullOrEmpty(delim) || m_szCursor >= m_memTarget.Length)
			{
				if (m_szCursor >= m_memTarget.Length) return string.Empty;
				// No delimiter, or delimiter is empty, return rest of string from cursor
				int startPos = m_szCursor;
				m_szCursor = m_memTarget.Length;
				return m_memTarget.Slice(startPos).ToString();
			}

			// Using m_strTarget for IndexOf to allow StringComparison.Ordinal
			// and to match original logic which implies it works on the full string representation
			int delimPos = m_strTarget.IndexOf(delim, m_szCursor, StringComparison.Ordinal);
			string result;

			if (delimPos != -1)
			{
				result = m_memTarget.Slice(m_szCursor, delimPos - m_szCursor).ToString();
				m_szCursor = delimPos;
			}
			else
			{
				result = m_memTarget.Slice(m_szCursor).ToString();
				m_szCursor = m_memTarget.Length;
			}
			return result;
		}


		public void movePastWhitespace()
		{
			ReadOnlySpan<char> s = m_memTarget.Span;
			int len = s.Length;
			while (m_szCursor < len && isSpace(s[m_szCursor]) && !isSymbol(s[m_szCursor]) && !isQuote(s[m_szCursor]))
			{
				m_szCursor++;
			}
		}

		// ───────────────────────────────────────────────────────── token (span)
		public ReadOnlySpan<char> PeekNextTokenSpan()
		{
			int originalCursor = m_szCursor;
			ReadOnlySpan<char> tokenSpan = GetNextTokenSpan();
			m_szCursor = originalCursor;
			return tokenSpan;
		}

		public ReadOnlySpan<char> GetNextTokenSpan()
		{
			ReadOnlySpan<char> s = m_memTarget.Span;
			int len = s.Length;
			int i = m_szCursor;

			// Skip leading whitespace (unless also symbol/quote)
			// This loop stops when a character is found that is:
			// - Not whitespace OR
			// - Is a symbol OR
			// - Is a quote
			while (i < len && isSpace(s[i]) && !isSymbol(s[i]) && !isQuote(s[i]))
			{
				i++;
			}

			if (i >= len)
			{
				m_szCursor = len; // Ensure cursor is at the end
				return ReadOnlySpan<char>.Empty;
			}

			char startChar = s[i];

			// 1-char symbol
			if (isSymbol(startChar))
			{
				m_szCursor = i + 1;
				return s.Slice(i, 1);
			}

			// Quoted block
			if (isQuote(startChar))
			{
				int j = i + 1; // Start looking for end quote after the opening quote
				while (j < len && s[j] != startChar)
				{
					j++;
				}
				// The token is content between quotes
				var slice = s.Slice(i + 1, j - (i + 1));
				m_szCursor = (j < len) ? j + 1 : len; // Move past closing quote if found, else to end
				return slice;
			}

			// Regular token (up to whitespace or symbol)
			int k = i;
			// Loop while char is not whitespace AND not a symbol
			// (Quotes are not considered separators for regular tokens here,
			// a quote char would have been handled by isQuote or !isSpace earlier if not also symbol/whitespace)
			while (k < len && !isSpace(s[k]) && !isSymbol(s[k]))
			{
				k++;
			}
			var token = s.Slice(i, k - i);
			m_szCursor = k;
			return token;
		}

		// string wrappers (allocate once per token)
		public string peekNextToken() => PeekNextTokenSpan().ToString();
		public string getNextToken() => GetNextTokenSpan().ToString();

		// ───────────────────────────────────────────────────────── split helper
		public List<string> split()
		{
			List<string> tokens = new();
			// Use the updated hasMoreTokens() which aligns with original's intent
			while (this.hasMoreTokens())
			{
				string token = getNextToken();
				// Original's split had a `?? throw`. If getNextToken can return null
				// (which it currently doesn't, it returns "" for empty tokens),
				// that would be needed. Given current GetNextTokenSpan() logic,
				// it should always produce a token string (possibly empty) if hasMoreTokens() is true.
				tokens.Add(token);
			}
			return tokens;
		}
	}
}
