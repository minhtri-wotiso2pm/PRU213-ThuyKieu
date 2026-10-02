// Kiểm thử tự động kịch bản Ink: biên dịch + chạy 3000 lượt ngẫu nhiên, báo lượt bị kẹt,
// tag lạ, danh sách speaker/emotion/camera/scene, phân bố ending và số lựa chọn ở Chương 4.
// Cách dùng:  npm install inkjs   rồi   node ink_branch_test.js <đường dẫn file .ink>
const fs=require("fs");
const {Compiler}=require("inkjs/full");
const src=fs.readFileSync(process.argv[2],"utf8");
const c=new Compiler(src);
let story;
try{ story=c.Compile(); }catch(e){ console.log("COMPILE FAIL", e.message); }
console.log("errors:",c.errors); console.log("warnings:",c.warnings);
if(!story) process.exit(1);
const json=story.ToJson();
const {Story}=require("inkjs/full");
// exhaustive-ish: random playthroughs with minigame results
const knownTags=new Set(["chapter","scene","music","sfx","speaker","emotion","camera","evidence","clue","minigame","gameplay","ui","ending","explore","req"]);
const endings={}, confront={}, unknown=new Set(), speakers=new Set(), emotions=new Set(), cams=new Set(), scenes=new Set();
let minTien=1e9, maxLines=0, minLines=1e9, choicesNoOpts=0;
for(let run=0; run<3000; run++){
  const s=new Story(json);
  let lines=0, lastEnding=null, lastConf=[];
  let guard=0;
  while(guard++<5000){
    while(s.canContinue){
      const t=s.Continue(); lines++;
      for(const tag of s.currentTags){
        const [k,v]=tag.split(":").map(x=>x.trim());
        if(!knownTags.has(k)) unknown.add(tag);
        if(k==="speaker")speakers.add(v); if(k==="emotion")emotions.add(v); if(k==="camera")cams.add(v); if(k==="scene")scenes.add(v);
        if(k==="ending") lastEnding=v;
        if(k==="minigame") s.variablesState["kq_minigame"]=Math.floor(Math.random()*4)-1;
      }
    }
    if(s.currentChoices.length===0) break;
    for(const ch of s.currentChoices) for(const tg of (ch.tags||[])){ if(!knownTags.has(tg.split(":")[0].trim())) unknown.add("choice#"+tg); }
    if(s.state.currentPathString===null){}
    const texts=s.currentChoices.map(c=>c.text);
    if(texts.some(t=>t.startsWith("Kể lại mọi chuyện"))) lastConf=texts.length;
    s.ChooseChoiceIndex(Math.floor(Math.random()*s.currentChoices.length));
  }
  if(s.canContinue||s.currentChoices.length) console.log("STUCK run",run);
  endings[lastEnding]=(endings[lastEnding]||0)+1;
  confront[lastConf]=(confront[lastConf]||0)+1;
  minTien=Math.min(minTien,s.variablesState["tien"]);
  maxLines=Math.max(maxLines,lines); minLines=Math.min(minLines,lines);
}
console.log({endings, confrontOptionCounts:confront, minTien, minLines, maxLines});
console.log("unknown tags:",[...unknown]);
console.log("speakers:",[...speakers].join(", "));
console.log("emotions("+emotions.size+"):",[...emotions].join(", "));
console.log("cameras("+cams.size+"):",[...cams].join(", "));
console.log("scenes("+scenes.size+"):",[...scenes].join(", "));
// sample line with inline conditional
