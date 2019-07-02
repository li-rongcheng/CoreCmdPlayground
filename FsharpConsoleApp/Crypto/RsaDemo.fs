﻿module RsaDemo

open System
open System.Security.Cryptography
open System.Text

// RSA encryption function
let RsaEncrypt (data : byte[]) (key : RSAParameters) (padding : bool) : byte[] =
    try
        let rsa = new RSACryptoServiceProvider()
        rsa.ImportParameters(key)
        rsa.Encrypt(data, padding)
    with
    | :? CryptographicException as e ->
        printfn "%A" e.Message
        Array.empty

// RSA decryption function
let RsaDecrypt (data : byte[]) (key : RSAParameters) (padding : bool) : byte[] =
    try
        let rsa = new RSACryptoServiceProvider()
        rsa.ImportParameters(key)
        rsa.Decrypt(data, padding)
    with
    | :? CryptographicException as e ->
        printfn "%A" e.Message
        Array.empty         
        
// Example
// ========================
let rsa = new RSACryptoServiceProvider(2048)
let publicPrivateKeyXML = rsa.ToXmlString(true)
rsa.FromXmlString(publicPrivateKeyXML)
let publicOnlyKeyXML = rsa.ToXmlString(false)
let plainText = "hello world"
let plainTextArray = System.Text.Encoding.ASCII.GetBytes(plainText)
let cipherArray = RsaEncrypt plainTextArray (rsa.ExportParameters(false)) false
let cipherText = System.Text.Encoding.ASCII.GetString(cipherArray)
let decryptedTextArray = RsaDecrypt (System.Text.Encoding.ASCII.GetBytes(cipherText)) (rsa.ExportParameters(true)) false
let decryptedText = System.Text.Encoding.ASCII.GetString(decryptedTextArray)