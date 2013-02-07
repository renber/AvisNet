using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Avis.Common
{

    /// <summary>
    /// An Elvin URI identifying an Elvin router as described in the "Elvin
    /// URI Scheme" specification at
    /// http://elvin.org/specs/draft-elvin-uri-prelim-02.txt. 
    /// </summary>
    /// <example>
    /// The most common Elvin URI for a TCP endpoint is of the form (sections in in []
    /// are optional):
    /// elvin:[version]/[protocol]/hostname[:port][;options]
    ///
    /// version:  protocol version major.minor form e.g. "4.0"
    /// protocol: protocol stack in transport,security,marshalling order
    /// e.g. "tcp,none,xdr". Alternatively the alias "secure"
    /// can be used to denote the default secure stack
    /// ("ssl,none,xdr").
    /// options:  name1=value1[;name2=value2]* e.g. foo=bar;black=white  
    /// Example URI 1: <code>elvin://localhost:2917</code>
    /// Example URI 2: <code>elvin://192.168.0.2:2917;foo=true</code>
    /// Example URI 3: <code>elvin:4.0/ssl,none,xdr/localhost:443</code>
    /// </example>    
    public class ElvinURI
    {        
        /// <summary>
        ///  True if this URI specifies secure TLS transport (protocol.Equals(SECURE_PROTOCOL)).
        /// </summary>
        public bool IsSecure
        {
            get
            { 
                return Protocol.SequenceEqual(Protocols.SecureProtocol);
            }
        }

        /**
         * Basic matcher for URI's. Key sections pulled out here: detail
         * parsing is done as a separate pass.
         */
        private static Regex UrlPattern =
            // NB: key sections of regexp are marked with | below
            //                |scheme|ver     |protocol|host:port  |options 
          new Regex("(\\w+):([^/]+)?/([^/]+)?/([^;/][^;]*)(;.*)?");

        /**
         * The original URI string as passed into the constructor.
         */
        public String UriString { get; private set; }

        /**
         * The URI scheme (i.e the part before the ":"). This must be 
         * "elvin" for URI's referring to Elvin routers.
         */
        public String Scheme { get; set; }

        /**
         * Major protocol version. Default is Common.ClientVersionMajor
         */
        public int VersionMajor { get; set; }

        /**
         * Minor protocol version. Default is Common.ClientVersionMinor
         */
        public int VersionMinor { get; set; }

        /**
         * The stack of protocol modules in (transport,security,marshalling)
         * order. e.g. "tcp", "none", "xdr"
         */
        public IList<String> Protocol { get; set; }

        /**
         * The host name.
         */
        public String Host { get; set; }

        /**
         * The port. Default is {@link Common#DEFAULT_PORT}.
         */
        public int Port { get; set; }

        /**
         * The URI options. e.g. elvin://host:port;option1=value1;option2=value2
         */
        public Dictionary<String, String> Options { get; private set; }

        private int hash;

        #region Constructors

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="uriString">The URI.</param>
        /// <exception cref="UriFormatException">If the URI is not valid.</exception>
        public ElvinURI(String uriString)
        {
            Init();

            this.UriString = uriString;

            ParseUri();

            Validate();

            this.hash = ComputeHash();
        }

        /// <summary>
        /// Create a new instance from a host and port using defaults for others.
        /// </summary>
        /// <param name="host">Host name or IP address</param>
        /// <param name="port">Port number.</param>
        public ElvinURI(String host, int port)
        {
            Init();

            this.UriString = "elvin://" + host + ':' + port;
            this.Scheme = "elvin";
            this.Host = host;
            this.Port = port;
            this.hash = ComputeHash();
        }


        /// <summary>
        /// Create a new instance using an existing URI for defaults.
        /// </summary>
        /// <param name="uriString">The URI string.</param>
        /// <param name="defaultUri">The URI to use for any values that are not specified by uriString.</param>
        /// <exception cref="UriFormatException">If the URI is not valid.</exception>
        public ElvinURI(String uriString, ElvinURI defaultUri)
        {
            Init(defaultUri);

            this.UriString = uriString;

            ParseUri();

            Validate();

            this.hash = ComputeHash();
        }

        /// <summary>
        /// Create a copy of a URI.
        /// </summary>
        /// <param name="defaultUri">The URI to copy.</param>
        public ElvinURI(ElvinURI defaultUri)
        {
            Init(defaultUri);

            Validate();
        }

        #endregion

        protected void Init(ElvinURI defaultUri)
        {
            this.UriString = defaultUri.UriString;
            this.Scheme = defaultUri.Scheme;
            this.VersionMajor = defaultUri.VersionMajor;
            this.VersionMinor = defaultUri.VersionMinor;
            this.Protocol = defaultUri.Protocol;
            this.Host = defaultUri.Host;
            this.Port = defaultUri.Port;
            this.Options = defaultUri.Options;
            this.hash = defaultUri.hash;
        }

        protected void Init()
        {
            this.Scheme = null;
            this.VersionMajor = Common.ClientVersionMajor;
            this.VersionMinor = Common.ClientVersionMinor;
            this.Protocol = Protocols.DefaultProtocol;
            this.Host = null;
            this.Port = Common.DefaultPort;
            this.Options = new Dictionary<String, String>();
        }

        private void Validate()
        {
            if (!ValidScheme(Scheme))
                throw new UriFormatException(UriString, new Exception("Invalid scheme: " + Scheme));
        }

        /// <summary>
        /// Check if scheme is valid. May be extended.
        /// </summary>
        /// <param name="schemeToCheck"></param>
        /// <returns></returns>
        protected bool ValidScheme(String schemeToCheck)
        {
            return schemeToCheck == "elvin";
        }

        public override string ToString()
        {
            return UriString;
        }

        /// <summary>
        /// Generate a canonical text version of this URI.
        /// </summary>
        /// <returns></returns>
        public String ToCanonicalString()
        {
            StringBuilder str = new StringBuilder();

            str.Append(Scheme).Append(':');

            str.Append(VersionMajor).Append('.').Append(VersionMinor);

            str.Append('/');

            str.Append(String.Join(",", Protocol));

            str.Append('/').Append(Host).Append(':').Append(Port);

            // NB: options is a sorted map, canonical order is automatic
            foreach (var option in Options)
            {
                str.Append(';');
                str.Append(option.Key).Append('=').Append(option.Value);
            }

            return str.ToString();
        }

        public override int GetHashCode()
        {
            return hash;
        }

        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            return obj is ElvinURI && Equals((ElvinURI)obj);
        }

        public bool Equals(ElvinURI uri)
        {
            return hash == uri.hash &&
                   Scheme == uri.Scheme &&
                   Host == uri.Host &&
                   Port == uri.Port &&
                   VersionMajor == uri.VersionMajor &&
                   VersionMinor == uri.VersionMinor &&
                    Enumerable.SequenceEqual(Options, uri.Options) &&
                    Enumerable.SequenceEqual(Protocol, uri.Protocol);
        }

        private int ComputeHash()
        {
            int pHash = Scheme.GetHashCode() ^ Host.GetHashCode() ^ Port;
            foreach (var p in Protocol)
            {
                pHash ^= p.GetHashCode();
            }

            return  pHash;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="UriFormatException"></exception>
        private void ParseUri()
        {
            if (!UrlPattern.IsMatch(UriString))
                throw new UriFormatException(UriString, new Exception("Not a valid Elvin URI"));

            var m = UrlPattern.Match(UriString);

            Scheme = m.Groups[1].Value;
            // version
            if (m.Groups[2].Success)
                ParseVersion(m.Groups[2].Value);
            // protocol
                 if (m.Groups[3].Success)
                     ParseProtocol(m.Groups[3].Value);
            // endpoint (host/port)
            ParseEndpoint(m.Groups[4].Value);

            if (m.Groups[5].Success)
               ParseOptions(m.Groups[5].Value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="versionExpr"></param>
        /// <exception cref="UriFormatException"></exception>
        private void ParseVersion(String versionExpr)
        {
            Regex versionMatch = new Regex("^(\\d+)(?:\\.(\\d+))?$");

            if (versionMatch.IsMatch(versionExpr))
            {
                try
                {
                    var m = versionMatch.Match(versionExpr);

                    VersionMajor = int.Parse(m.Groups[1].Value);

                    if (m.Groups[2].Success)
                        VersionMinor = int.Parse(m.Groups[2].Value);
                }
                catch (FormatException ex)
                {
                    throw new UriFormatException(UriString,
                                                 new Exception("Number too large in version string: \"" +
                                                 versionExpr + "\""));
                }
            }
            else
            {
                throw new UriFormatException(UriString,
                                             new Exception("Invalid version string: \"" + versionExpr + "\""));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="protocolExpr"></param>
        /// <exception cref="uriFormatException"></exception>
        private void ParseProtocol(String protocolExpr)
        {
            Match protocolMatch = new Regex("^(?:(\\w+),(\\w+),(\\w+))$|^secure$").Match(protocolExpr);

            if (protocolMatch.Success)
            {
                if (protocolMatch.Groups[1].Success)
                    Protocol = protocolExpr.Split(',').ToList();
                else
                    Protocol = Protocols.SecureProtocol;
            }
            else
            {
                throw new UriFormatException(UriString,
                                               new Exception("Invalid protocol: \"" +
                                               protocolExpr + "\""));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="endpoint"></param>
        /// <exception cref="uriFormatException"></exception>
        private void ParseEndpoint(String endpoint)
        {
            Regex pattern;

            // choose between IPv6 and IPv4 address scheme
            if (endpoint[0] == '[')
                pattern = new Regex("^(\\[[^\\]]+\\])(?::(\\d+))?$");
            else
                pattern = new Regex("^([^:]+)(?::(\\d+))?$");

            var endpointMatch = pattern.Match(endpoint);

            if (endpointMatch.Success)
            {
                Host = endpointMatch.Groups[1].Value;
                
                if (endpointMatch.Groups[2].Success)
                    Port = int.Parse(endpointMatch.Groups[2].Value);
            }
            else
            {
                throw new UriFormatException(UriString, new Exception("Invalid port number"));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="optionsExpr"></param>
        /// <exception cref="UriFormatException"></exception>
        private void ParseOptions(String optionsExpr)
        {
            var optionMatch = new Regex(";([^=;]+)=([^=;]*)").Matches(optionsExpr);

            Options = new Dictionary<String, String>();

            //int index = 0;

            foreach (Match m in optionMatch.OfType<Match>().OrderByDescending(x => x.Index))
            {
                Options.Add(m.Groups[1].Value, m.Groups[2].Value);

                optionsExpr = optionsExpr.Remove(m.Index, m.Length);
            }

            // not all parts were parameters
            if (optionsExpr != String.Empty)
            {
                throw new UriFormatException(UriString, new Exception("Invalid options: \"" + optionsExpr + "\""));
            }
        }
    }
}