
<script lang="ts">
    let { data } = $props();

    import CodeEditor from '$lib/components/code-editor.svelte';

    let content = $state('<p>Hello, world!</p>');
    let compiledContent = $state('<p>Hello, world!</p>');
    
    $effect(() => {
        thing()
    })
    
    async function thing() {
        const thing = await fetch(`${data.post}/render`, {
            method: 'POST',
            body: JSON.stringify({ content: content }),
            headers: {
                'Content-Type': 'application/json'
            }
        });

        const json = await thing.json();
        compiledContent = json.content;
    }
</script>

<h1>Welcome to SvelteKit</h1>
<p>Visit <a href="https://svelte.dev/docs/kit">svelte.dev/docs/kit</a> to read the documentation</p>
<div>{data.post}</div>


<div class="fixed-grid">
    <div class="grid">
        <div class="cell">
            <CodeEditor bind:value={content} />
        </div>
        <div class="cell">
            <CodeEditor bind:value={compiledContent} />
        </div>
    </div>
</div>
