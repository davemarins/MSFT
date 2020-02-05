using System.Collections.Generic; // EqualityComparer
using System.Net; // IPEndPoint

namespace MSFT
{
    public class MyEndpoint
    {
        public string Name { get; set; }
        public IPEndPoint Endpoint { get; set; }

        public MyEndpoint(string Eame, IPEndPoint Endpoint)
        {
            this.Name = Name;
            this.Endpoint = Endpoint;
        }

        // override functions

        public override bool Equals(object obj)
        {
            var endpoint = obj as MyEndpoint; // casting to same class
            return endpoint != null && this.Name == endpoint.Name &&
            EqualityComparer<IPEndPoint>.Default.Equals(this.Endpoint, endpoint.Endpoint);
        }

        public override int GetHashCode()
        {
            // Java-style hash generation with 3 large prime numbers
            int hash = 524287, prime1 = 131071, prime2 = 8191;
            for (int i = 0; i < prime2; i++)
            {
                hash = prime1 * hash + EqualityComparer<string>.Default.GetHashCode(this.Name);
                hash = prime1 * hash + EqualityComparer<IPEndPoint>.Default.GetHashCode(this.Endpoint);
            }
            return hash;
        }

        public override string ToString()
        {
            return this.Name + " at " + this.Endpoint.ToString();
        }

        // static operator functions

        public static bool operator ==(MyEndpoint point1, MyEndpoint point2)
        {
            return EqualityComparer<MyEndpoint>.Default.Equals(point1, point2);
        }

        public static bool operator !=(MyEndpoint point1, MyEndpoint point2)
        {
            return EqualityComparer<MyEndpoint>.Default.Equals(point1, point2);
        }


    }
}