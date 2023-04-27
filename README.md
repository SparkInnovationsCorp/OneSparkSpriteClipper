<h1>OneSparkSpriteClipper</h1><p>OneSparkSpriteClipper is a .NET console application that extracts the clipping paths from a sprite image and saves them to a JSON file. The application uses a flood-fill algorithm to find the non-transparent areas of the image and create clipping paths around them. The resulting JSON file can be used to create individual sprites from the original sprite image.</p><h2>Usage</h2><pre><div class="bg-black rounded-md mb-4"><div class="flex items-center relative text-gray-200 bg-gray-800 px-4 py-2 text-xs font-sans justify-between rounded-t-md"><span>php</span><button class="flex ml-auto gap-2"><svg stroke="currentColor" fill="none" stroke-width="2" viewBox="0 0 24 24" stroke-linecap="round" stroke-linejoin="round" class="h-4 w-4" height="1em" width="1em" xmlns="http://www.w3.org/2000/svg"><path d="M16 4h2a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h2"></path><rect x="8" y="2" width="8" height="4" rx="1" ry="1"></rect></svg>Copy code</button></div><div class="p-4 overflow-y-auto"><code class="!whitespace-pre hljs language-php-template"><span class="xml">OneSparkSpriteClipper <span class="hljs-tag">&lt;<span class="hljs-name">sourceImage</span>&gt;</span> <span class="hljs-tag">&lt;<span class="hljs-name">destinationPath</span>&gt;</span>
</span></code></div></div></pre><ul><li><code>sourceImage</code>: The path to the source sprite image.</li><li><code>destinationPath</code>: (optional) The path where the resulting JSON file will be saved. If not specified, the JSON file will be saved in the same directory as the source image with the same name as the image.</li></ul><h2>Output</h2><p>The application outputs a JSON file that contains an array of clipping paths. Each clipping path is represented by a <code>name</code> and an array of <code>clipPath</code> coordinates.</p><h2>Dependencies</h2><ul><li>.NET Framework 4.7.2 or later</li></ul>
