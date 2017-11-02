using System.Collections;

namespace Assets.Scripts.Math
{
    public static class Primes
    {
        public static BitArray ESieve( int upperLimit )
        {
            int sieveBound     = upperLimit - 1;
            int upperSqrt      = (int)System.Math.Sqrt(sieveBound);
            BitArray PrimeBits = new BitArray(sieveBound + 1, true);

            PrimeBits[ 0 ] = false;
            PrimeBits[ 1 ] = false;

            for( int j = 4; j <= sieveBound; j += 2 ) {
                PrimeBits[ j ] = false;
            }

            for( int i = 3; i <= upperSqrt; i += 2 ) {
                if( PrimeBits[ i ] ) {
                    int inc = i * 2;

                    for( int j = i * i; j <= sieveBound; j += inc ) {
                        PrimeBits[ j ] = false;
                    }
                }
            }

            return PrimeBits;
        }

        public static int FindNextPrime( int number )
        {
            BitArray IsPrime = ESieve(1000000);

            number++;
            for( ; number < IsPrime.Length; number++ ) {
                //found a prime return that number
                if( IsPrime[ number ] ) {
                    return number;
                }
            }
            //no prime return error code
            return -1;
        }
    }
}
