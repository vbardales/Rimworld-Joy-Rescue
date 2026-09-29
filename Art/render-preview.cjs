// Run with Node.js and playwright/sharp available through NODE_PATH.
const fs=require('fs'),path=require('path'),http=require('http');
const {chromium}=require('playwright'),sharp=require('sharp');
const art=__dirname;
const rgb=h=>h.match(/\w\w/g).map(x=>parseInt(x,16));
const lum=c=>c.map(v=>{v/=255;return v<=.04045?v/12.92:((v+.055)/1.055)**2.4}).reduce((s,v,i)=>s+v*[.2126,.7152,.0722][i],0);
const contrast=(a,b)=>(Math.max(lum(a),lum(b))+.05)/(Math.min(lum(a),lum(b))+.05);
(async()=>{
 const p=JSON.parse(fs.readFileSync(path.join(art,'preview-palette.json')));
 const l=JSON.parse(fs.readFileSync(path.join(art,'preview-layout.json')));
 const about=fs.readFileSync(path.join(art,'../Mod/About/About.xml'),'utf8');
 const versions=[...about.match(/<supportedVersions>([\s\S]*?)<\/supportedVersions>/)[1].matchAll(/<li>(\d+\.\d+)<\/li>/g)].map(m=>m[1]);
 versions.sort((a,b)=>{const x=a.split('.').map(Number),y=b.split('.').map(Number);return y[0]-x[0]||y[1]-x[1]});
 if(l.version!==versions[0])throw Error('Update layout version to '+versions[0]);
 const server=http.createServer((req,res)=>{
   const file=path.join(art,path.basename(req.url.split('?')[0])||'preview.html');
   res.setHeader('Content-Type',file.endsWith('.json')?'application/json':file.endsWith('.png')?'image/png':'text/html');
   fs.createReadStream(file).on('error',()=>{res.statusCode=404;res.end()}).pipe(res);
 });
 await new Promise(r=>server.listen(0,'127.0.0.1',r));
 let browser;
 try{
 browser=await chromium.launch({channel:'chrome',headless:true});
 const page=await browser.newPage({viewport:{width:l.width,height:l.height},deviceScaleFactor:1});
 await page.goto(`http://127.0.0.1:${server.address().port}/preview.html`);await page.evaluate(()=>window.ready);
 const cdp=await page.context().newCDPSession(page);await cdp.send('DOM.enable');await cdp.send('CSS.enable');
 const {root}=await cdp.send('DOM.getDocument');const fonts={},rects={};
 for(const selector of ['h1','.summary','.version']){
 const {nodeId}=await cdp.send('DOM.querySelector',{nodeId:root.nodeId,selector});
 fonts[selector]=(await cdp.send('CSS.getPlatformFontsForNode',{nodeId})).fonts;
 if(!fonts[selector].every(f=>/^Segoe UI(?: Semibold)?$/.test(f.familyName)))throw Error('Unexpected font: '+JSON.stringify(fonts[selector]));
 rects[selector]=await page.locator(selector).boundingBox();
 }
 const output=path.join(art,process.env.PREVIEW_OUTPUT||'../Mod/About/Preview.png');
 const rendered=await page.screenshot();await sharp(rendered).png({compressionLevel:9}).toFile(output);
 await sharp(rendered).resize({width:268}).png().toFile(path.join(art,'preview-268.png'));
 await page.addStyleTag({content:'.copy,.version{visibility:hidden}'});
 const background=await page.screenshot();await fs.promises.writeFile(path.join(art,'preview-background.png'),background);
 const {data,info}=await sharp(background).removeAlpha().raw().toBuffer({resolveWithObject:true});
 const results={};
 for(const sel of ['h1','.summary']){
 let min=Infinity;const r=rects[sel];
 for(let y=Math.floor(r.y);y<Math.ceil(r.y+r.height);y++)for(let x=Math.floor(r.x);x<Math.ceil(r.x+r.width);x++){
 const i=(y*info.width+x)*3;min=Math.min(min,contrast(rgb(p.inkPrimary),[data[i],data[i+1],data[i+2]]));
 }results[sel]=min;if(min<4.5)throw Error('Insufficient contrast '+sel+': '+min);
 }
 results.badge=contrast(rgb(p.badgeInk),rgb(p.accent));if(results.badge<4.5)throw Error('Badge contrast');
 if(fs.statSync(output).size>=900000)throw Error('Preview exceeds size limit');
 const report={fonts,rects,minimumContrastAcrossEntireTextRectangles:results,tag:'Not applicable: original public mod, no tag',bytes:fs.statSync(output).size,width:l.width,height:l.height,version:versions[0],fontsReady:true};
 fs.writeFileSync(path.join(art,'preview-qa.json'),JSON.stringify(report,null,2)+'\n');console.log(JSON.stringify(report,null,2));
 }finally{if(browser)await browser.close();server.close();}
})().catch(e=>{console.error(e);process.exitCode=1});
