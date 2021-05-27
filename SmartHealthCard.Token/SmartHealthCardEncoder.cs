﻿using SmartHealthCard.Token.Algorithms;
using SmartHealthCard.Token.Model.Shc;
using SmartHealthCard.Token.Serializers.Json;
using SmartHealthCard.Token.Serializers.Jws;
using SmartHealthCard.Token.Serializers.Shc;
using SmartHealthCard.Token.JwsToken;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace SmartHealthCard.Token
{
  /// <summary>
  /// A SMART Health Card encoder. 
  /// Take a SMART Health Card payload as an oject model and a and encoding it into a SMART Health Card JWS Token
  /// It can also take a list of SMART Health Card payloads and return the many as SMART Health Card JWS 
  /// Tokens in a .smart-health-card JSON flie format  
  /// </summary>
  public class SmartHealthCardEncoder
  {
    private readonly IJsonSerializer JsonSerializer;
    private readonly IJwsHeaderSerializer HeaderSerializer;
    private readonly IJwsPayloadSerializer PayloadSerializer;

    /// <summary>
    /// Default Constructor
    /// </summary>
    public SmartHealthCardEncoder()
    {
      this.JsonSerializer = new JsonSerializer();
      HeaderSerializer = new SmartHealthCardJwsHeaderSerializer(JsonSerializer);
      this.PayloadSerializer = new SmartHealthCardJwsPayloadSerializer(JsonSerializer);
    }

    /// <summary>
    /// Provide any implementation of the follwowing interfaces to overide their default implementation
    /// </summary>
    /// <param name="JsonSerializer">Provides basic JSON serialization</param>
    /// <param name="HeaderSerializer">Provides the serialization of the data that is packed into the JWS Header</param>
    /// <param name="PayloadSerializer">Provides the serialization of the data that is packed into the JWS Payload</param>
    public SmartHealthCardEncoder(IJsonSerializer? JsonSerializer, IJwsHeaderSerializer? HeaderSerializer, IJwsPayloadSerializer? PayloadSerializer)
    {
      this.JsonSerializer = JsonSerializer ?? new JsonSerializer();
      this.HeaderSerializer = HeaderSerializer ?? new SmartHealthCardJwsHeaderSerializer(this.JsonSerializer);
      this.PayloadSerializer = PayloadSerializer ?? new SmartHealthCardJwsPayloadSerializer(this.JsonSerializer);
    }

    /// <summary>
    /// Get a SMART Health Card JWS Token 
    /// Requires a Certifiacte containing a private Elliptic Curve key using the P-256 curve
    /// Requires a SMART Health Card payload in an object model form    
    /// </summary>
    /// <param name="Certificate">Certifiacte containing a private Elliptic Curve key using the P-256 curve</param>
    /// <param name="SmartHealthCard">SMART Health Card payload in an object model form</param>
    /// <returns></returns>
    public string GetToken(X509Certificate2 Certificate, SmartHealthCardModel SmartHealthCard)
    {      
      //Create the Elliptic Curve Signing Algorithm
      IAlgorithm Algorithm = new ES256Algorithm(Certificate, JsonSerializer);
      SmartHealthCareJWSHeaderModel Header = GetHeader(Algorithm);
      IJwsEncoder JwsEncoder = GetEncoder(Certificate, Algorithm);
      return JwsEncoder.Encode(Header, SmartHealthCard);
    }

    /// <summary>
    /// Get many SMART Health Card JWS Tokens in a .smart-health-card JSON flie format 
    /// Requires a Certifiacte containing a private Elliptic Curve key using the P-256 curve
    /// Requires a SMART Health Card payload in an object model form  
    /// </summary>
    /// <param name="Certificate">Certifiacte containing a private Elliptic Curve key using the P-256 curve</param>
    /// <param name="SmartHealthCardList">List of SMART Health Card payload in an object model form</param>
    /// <returns></returns>
    public string GetSmartHealthCardFile(X509Certificate2 Certificate, List<SmartHealthCardModel> SmartHealthCardList)
    {
      //Create the Elliptic Curve Signing Algorithm
      IAlgorithm Algorithm = new ES256Algorithm(Certificate, JsonSerializer);
      SmartHealthCareJWSHeaderModel Header = GetHeader(Algorithm);
      IJwsEncoder JwsEncoder = GetEncoder(Certificate, Algorithm);

      //Smart Health Card File object model holds many tokens
      SmartHealthCardFile SmartHealthCardFile = new SmartHealthCardFile();
      foreach (SmartHealthCardModel SmartHealthCard in SmartHealthCardList)
      {
        SmartHealthCardFile.VerifiableCredentialList.Add(JwsEncoder.Encode(Header, SmartHealthCard));
      }

      return JsonSerializer.ToJson(SmartHealthCardFile);     
    }

    private IJwsEncoder GetEncoder(X509Certificate2 Certificate, IAlgorithm Algorithm)
    {
      //Encode the JWS Token passing in the Header and Payload byte arrays from our two custom serializers 
      return new SmartHealthCardJwsEncoder(HeaderSerializer, PayloadSerializer, Algorithm);
    }

    private SmartHealthCareJWSHeaderModel GetHeader(IAlgorithm Algorithm)
    {
      //Create the Smart Health Card JWS Header Model
      return new SmartHealthCareJWSHeaderModel(Algorithm.Name, "DEF", Algorithm.GetKid());
    }
  }
}
