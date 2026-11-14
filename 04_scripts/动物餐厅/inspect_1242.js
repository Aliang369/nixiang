const fs=require('fs'),path=require('path'),pako=require('pako'),xxtea=require('xxtea-node');
const input=process.argv[2],out=process.argv[3];
const b=fs.readFileSync(input),d=xxtea.decrypt(b,xxtea.toBytes('431FEWJOTe-gecTW'));
if(!d)throw Error('decrypt fail');const js=Buffer.from(pako.ungzip(new Uint8Array(d))).toString('utf8');
fs.writeFileSync(out,js);console.log(JSON.stringify({input,bytes:b.length,jsBytes:Buffer.byteLength(js),sha256:require('crypto').createHash('sha256').update(js).digest('hex')},null,2));
