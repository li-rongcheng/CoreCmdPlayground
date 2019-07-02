﻿module AesFileEncryption

open System
open System.IO
open System.Security.Cryptography
open System.Text

// Encrypt
let encrypt (key : string) inFile outFile = 
    use aes = new RijndaelManaged()
    use encryptor = aes.CreateEncryptor(ASCIIEncoding.UTF8.GetBytes(key), ASCIIEncoding.UTF8.GetBytes(key))
    use fsOut = new FileStream(outFile, FileMode.Create)
    use cs = new CryptoStream(fsOut, encryptor, CryptoStreamMode.Write)
    let buf = File.ReadAllBytes(inFile)
    printfn "%A" buf.Length
    cs.Write(buf, 0, buf.Length)
    cs.FlushFinalBlock()
    fsOut.Close()

// Decrypt
let decrypt (key : string) inFile outFile = 
    use aes = new RijndaelManaged()
    use decryptor = aes.CreateDecryptor(ASCIIEncoding.UTF8.GetBytes(key), ASCIIEncoding.UTF8.GetBytes(key))
    use fsIn = new FileStream(inFile, FileMode.Open)
    printfn "%A" fsIn.Length
    let cs = new CryptoStream(fsIn, decryptor, CryptoStreamMode.Read)
    let (buf : byte[]) = Array.zeroCreate (int fsIn.Length)
    cs.Read(buf, 0, buf.Length)
    File.WriteAllBytes(outFile, buf)