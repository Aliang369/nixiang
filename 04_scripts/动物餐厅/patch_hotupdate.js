const fs=require('fs'),path=require('path');
const work=process.argv[2];let source=fs.readFileSync(path.join(__dirname,'patch_1242.js'),'utf8');
source=source.replaceAll('index.original.js','device.hotupdate.js').replaceAll('main.original.js','device.main.js').replace('d93c4c17f0fe35806528cbc94b1112c886f3c298f8d290188a2cbda0f8bb5759','1da0378f823e435b2d2b2c6ff2fef9ceaf888a3241aaca3c4143e620d6a16535').replace('468b217c0b93fc69f6c79deccf0b5b6b88362619329ca1f48d3d68759c0491c0','8d66d859d6ae0fc9cb77a52b6b4f2207ceb232d6a547a7dcd6fca22ec21a5a96');
source=source.replaceAll("name+'.patched", "name+'.hot.patched").replaceAll("'patch_report.json'","'hot_patch_report.json'");
new Function('require','process','__dirname',source)(require,{argv:['node','patch',work],env:process.env},__dirname);
