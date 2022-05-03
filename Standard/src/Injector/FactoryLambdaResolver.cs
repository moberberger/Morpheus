namespace Morpheus.DependencyInjection
{
    public class FactoryLambdaCreator : IResolver
    {
        Func<object> lambdaNoParam;
        Func<object, object[]> lambdaWithParam;
        public FactoryLambdaCreator( Func<object> factoryLambda ) => lambdaNoParam = factoryLambda;
        public FactoryLambdaCreator( Func<object, object[]> factoryLambda ) => lambdaWithParam = factoryLambda;

        public object Get( object[] @params )
        {
            if (lambdaNoParam != null)
                return lambdaNoParam();
            else
                return lambdaWithParam( @params );
        }
    }
}
