using Hl7.Fhir.Model;
using SmartHealthCard.Test.Model;
using SmartHealthCard.Test.Serializers;
using SmartHealthCard.Test.Support;
using SmartHealthCard.Token;
using SmartHealthCard.Token.Model.Shc;
using SmartHealthCard.Token.Providers;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace SmartHealthCard.Test
{
  public class SmartHealthCardDecoderTest
  {
    [Fact]
    public void Decode_Token_Verify_with_JWKS()
    {
      //### Prepare ######################################################
      //Get the ECC certificate from the Windows Certificate Store by Thumbprint      
      X509Certificate2 Certificate = CertificateSupport.GetCertificate(Thumbprint: "72c78a3460fb27b9ef2ccfae2538675b75363fee");
      List<X509Certificate2> CertificateList = new List<X509Certificate2>() { Certificate };

      //The base of the Url where a validator will retive the public keys from (e.g : [Issuer]/.well-known/jwks.json) 
      Uri Issuer = new Uri("https://sonichealthcare.com/something");
      string SmartHealthCardJwsToken = SmartHealthCardJwsSupport.GetJWSCovidExampleOne(Certificate, Issuer);
      
      //This testing JwksSupport class provides us wiht a mocked IJwksProvider that will inject the JWKS file
      //rather thnan make the HTTP call to go get it from a public endpoint.
      IJwksProvider MockedIJwksProvider = JwksSupport.GetMockedIJwksProvider(Certificate, Issuer);

      //Instantiate the SmartHealthCard Decoder
      SmartHealthCardDecoder Decoder = new SmartHealthCardDecoder(MockedIJwksProvider);

      //### Act #######################################################
      SmartHealthCardModel SmartHealthCardModel = Decoder.Decode(SmartHealthCardJwsToken, Verify: true);

      //### Assert #######################################################

      Assert.True(!string.IsNullOrWhiteSpace(SmartHealthCardJwsToken));
      Assert.NotNull(SmartHealthCardModel);      
    }

    [Fact]
    public void Decode_Token_Verify_with_Certificate()
    {
      //### Prepare ######################################################
      //Get the ECC certificate from the Windows Certificate Store by Thumbprint      
      X509Certificate2 Certificate = CertificateSupport.GetCertificate(Thumbprint: "72c78a3460fb27b9ef2ccfae2538675b75363fee");

      //The base of the Url where a validator will retive the public keys from (e.g : [Issuer]/.well-known/jwks.json) 
      Uri Issuer = new Uri("https://sonichealthcare.com/something");
      string SmartHealthCardJwsToken = SmartHealthCardJwsSupport.GetJWSCovidExampleOne(Certificate, Issuer);

      //This testing JwksSupport class provides us wiht a mocked IJwksProvider that will inject the JWKS file
      //rather thnan make the HTTP call to go get it from a public endpoint.
      IJwksProvider MockedIJwksProvider = JwksSupport.GetMockedIJwksProvider(Certificate, Issuer);

      //Instantiate the SmartHealthCard Decoder
      SmartHealthCardDecoder Decoder = new SmartHealthCardDecoder(MockedIJwksProvider);

      //### Act #######################################################

      //Verify and Decode
      SmartHealthCardModel SmartHealthCardModel = Decoder.Decode(SmartHealthCardJwsToken, Verify: true);

      //### Assert #######################################################

      Assert.True(!string.IsNullOrWhiteSpace(SmartHealthCardJwsToken));
      Assert.NotNull(SmartHealthCardModel);
    }


  }
}
