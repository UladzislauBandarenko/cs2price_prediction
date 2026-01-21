const API_BASE = 'http://localhost:8087/api';

// State
let selectedWeaponTypeId = null;
let selectedWeaponId = null;
let selectedSkinId = null;
let selectedWearTierId = null;
let selectedPattern = null;
let selectedStickers = [];
let stickerSearchTimeout = null;

// Store last prediction data
let lastPredictionData = null;
let lastPredictedPrice = null;

// DOM Elements
const weaponTypeSelect = document.getElementById('weaponType');
const weaponSelect = document.getElementById('weapon');
const skinSelect = document.getElementById('skin');
const wearTierSelect = document.getElementById('wearTier');
const patternSelect = document.getElementById('pattern');
const stickerSearchInput = document.getElementById('stickerSearch');
const stickerResultsDiv = document.getElementById('stickerResults');
const stickerListDiv = document.getElementById('stickerList');
const floatValueInput = document.getElementById('floatValue');
const statTrakCheckbox = document.getElementById('statTrak');
const predictBtn = document.getElementById('predictBtn');
const explainBtn = document.getElementById('explainBtn');
const explainV2Btn = document.getElementById('explainV2Btn');
const errorDiv = document.getElementById('error');
const resultDiv = document.getElementById('result');
const predictionInfoDiv = document.getElementById('predictionInfo');
const stickerFeaturesDiv = document.getElementById('stickerFeatures');
const aiExplanationDiv = document.getElementById('aiExplanation');
const aiExplanationV2Div = document.getElementById('aiExplanationV2');

// Utility functions
function showStep(stepNumber) {
    document.getElementById(`step${stepNumber}`).classList.remove('hidden');
}

function hideStepsFrom(stepNumber) {
    for (let i = stepNumber; i <= 8; i++) {
        document.getElementById(`step${i}`).classList.add('hidden');
    }
}

function hideError() {
    errorDiv.classList.add('hidden');
}

function showError(message) {
    errorDiv.textContent = message;
    errorDiv.classList.remove('hidden');
}

// Step 1: Load weapon types
async function loadWeaponTypes() {
    try {
        const response = await fetch(`${API_BASE}/meta/weapon-types`);
        if (!response.ok) throw new Error('Failed to fetch weapon types');
        
        const data = await response.json();
        
        weaponTypeSelect.innerHTML = '<option value="">Select a weapon type...</option>';
        data.forEach(type => {
            const option = document.createElement('option');
            option.value = type.id;
            option.textContent = type.name;
            weaponTypeSelect.appendChild(option);
        });
    } catch (error) {
        showError('Failed to load weapon types: ' + error.message);
    }
}

// Step 2: Load weapons
async function loadWeapons(weaponTypeId) {
    try {
        const response = await fetch(`${API_BASE}/meta/weapon-types/${weaponTypeId}/weapons`);
        if (!response.ok) throw new Error('Failed to fetch weapons');
        
        const data = await response.json();
        
        weaponSelect.innerHTML = '<option value="">Select a weapon...</option>';
        data.forEach(weapon => {
            const option = document.createElement('option');
            option.value = weapon.id;
            option.textContent = weapon.name;
            weaponSelect.appendChild(option);
        });
        showStep(2);
    } catch (error) {
        showError('Failed to load weapons: ' + error.message);
    }
}

// Step 3: Load skins
async function loadSkins(weaponId) {
    try {
        const response = await fetch(`${API_BASE}/meta/weapons/${weaponId}/skins`);
        if (!response.ok) throw new Error('Failed to fetch skins');
        
        const data = await response.json();
        
        skinSelect.innerHTML = '<option value="">Select a skin...</option>';
        data.forEach(skin => {
            const option = document.createElement('option');
            option.value = skin.id;
            option.textContent = skin.name;
            skinSelect.appendChild(option);
        });
        showStep(3);
    } catch (error) {
        showError('Failed to load skins: ' + error.message);
    }
}

// Step 4: Load wear tiers
async function loadWearTiers(skinId) {
    try {
        const response = await fetch(`${API_BASE}/meta/skins/${skinId}/wear-tiers`);
        if (!response.ok) throw new Error('Failed to fetch wear tiers');
        
        const data = await response.json();
        
        wearTierSelect.innerHTML = '<option value="">Select wear tier...</option>';
        data.forEach(tier => {
            const option = document.createElement('option');
            option.value = tier.id;
            option.textContent = tier.name;
            wearTierSelect.appendChild(option);
        });
        showStep(4);
    } catch (error) {
        showError('Failed to load wear tiers: ' + error.message);
    }
}

// Step 5: Load patterns
async function loadPatterns(skinId) {
    try {
        const response = await fetch(`${API_BASE}/meta/skins/${skinId}/patterns`);
        if (!response.ok) {
            // No patterns available, skip to next step
            patternSelect.innerHTML = '<option value="">No specific pattern</option>';
            showStep(5);
            showStep(6);
            return;
        }
        
        const data = await response.json();
        
        patternSelect.innerHTML = '<option value="">No specific pattern</option>';
        data.forEach(pattern => {
            const option = document.createElement('option');
            option.value = pattern.id;
            option.textContent = pattern.name;
            patternSelect.appendChild(option);
        });
        showStep(5);
    } catch (error) {
        // Patterns are optional, continue anyway
        patternSelect.innerHTML = '<option value="">No specific pattern</option>';
        showStep(5);
    }
}

// Step 6: Search stickers
async function searchStickers(query) {
    try {
        const url = query 
            ? `${API_BASE}/meta/stickers?q=${encodeURIComponent(query)}&limit=20`
            : `${API_BASE}/meta/stickers?limit=20`;
            
        const response = await fetch(url);
        if (!response.ok) throw new Error('Failed to fetch stickers');
        
        const data = await response.json();
        
        stickerResultsDiv.innerHTML = '';
        data.forEach(sticker => {
            // Don't show already selected stickers
            if (selectedStickers.some(s => s.id === sticker.id)) return;
            
            const div = document.createElement('div');
            div.className = 'sticker-item';
            div.textContent = sticker.name;
            div.onclick = () => addSticker(sticker);
            stickerResultsDiv.appendChild(div);
        });
        
        stickerResultsDiv.classList.add('active');
    } catch (error) {
        console.error('Failed to search stickers:', error);
    }
}

function addSticker(sticker) {
    if (selectedStickers.length >= 4) {
        showError('Maximum 4 stickers allowed');
        return;
    }
    
    if (selectedStickers.some(s => s.id === sticker.id)) {
        return;
    }
    
    selectedStickers.push(sticker);
    renderSelectedStickers();
    stickerSearchInput.value = '';
    stickerResultsDiv.classList.remove('active');
}

function removeSticker(stickerId) {
    selectedStickers = selectedStickers.filter(s => s.id !== stickerId);
    renderSelectedStickers();
}

function renderSelectedStickers() {
    const hint = document.querySelector('.selected-stickers .hint');
    hint.textContent = `Selected stickers (${selectedStickers.length}/4):`;
    
    stickerListDiv.innerHTML = '';
    selectedStickers.forEach(sticker => {
        const div = document.createElement('div');
        div.className = 'selected-sticker';
        div.innerHTML = `
            <span>${sticker.name}</span>
            <button class="remove-sticker" onclick="removeSticker(${sticker.id})">×</button>
        `;
        stickerListDiv.appendChild(div);
    });
}

// Make removeSticker available globally
window.removeSticker = removeSticker;

// Step 7-8: Make prediction
async function makePrediction() {
    hideError();
    resultDiv.classList.add('hidden');
    explainBtn.classList.add('hidden');
    explainV2Btn.classList.add('hidden');
    aiExplanationDiv.classList.add('hidden');
    aiExplanationV2Div.classList.add('hidden');
    
    const floatValue = parseFloat(floatValueInput.value);
    const isStattrak = statTrakCheckbox.checked;
    
    if (isNaN(floatValue) || floatValue < 0 || floatValue > 1) {
        showError('Please enter a valid float value between 0 and 1');
        return;
    }
    
    try {
        // Build request body conditionally
        const requestBody = {
            skinId: parseInt(selectedSkinId),
            wearTierId: selectedWearTierId ? parseInt(selectedWearTierId) : null,
            floatValue: floatValue,
            isStattrak: isStattrak,
            stickers: selectedStickers.map(s => s.id)
        };
        
        // Only add pattern if user selected one
        if (selectedPattern && selectedPattern !== "") {
            requestBody.pattern = parseInt(selectedPattern);
        }
        
        console.log('Sending prediction request:', requestBody);
        
        predictBtn.disabled = true;
        predictBtn.textContent = 'Predicting...';
        
        const response = await fetch(`${API_BASE}/predict`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(requestBody)
        });
        
        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(errorText);
        }
        
        const data = await response.json();
        console.log('Prediction response:', data);
        
        // Store prediction data for AI explanation
        lastPredictionData = requestBody;
        lastPredictedPrice = data.predicted_price;
        
        // Display result
        const weaponName = weaponSelect.options[weaponSelect.selectedIndex].text;
        const skinName = skinSelect.options[skinSelect.selectedIndex].text;
        const wearTierName = wearTierSelect.options[wearTierSelect.selectedIndex].text;
        const statTrakText = isStattrak ? 'StatTrak™ ' : '';
        
        predictionInfoDiv.innerHTML = `
            <div><strong>${statTrakText}${weaponName} | ${skinName}</strong></div>
            <div>Wear: ${wearTierName} (Float: ${floatValue.toFixed(4)})</div>
            ${requestBody.pattern ? `<div>Pattern: ${patternSelect.options[patternSelect.selectedIndex].text}</div>` : ''}
            ${selectedStickers.length > 0 ? `<div>Stickers: ${selectedStickers.length}</div>` : ''}
            <span class="price">$${data.predicted_price.toFixed(2)}</span>
        `;
        
        // Display sticker features EXACTLY from backend response
        if (data.stickers_features && data.stickers_features.stickers_count > 0) {
            stickerFeaturesDiv.innerHTML = `
                <h3>📊 Sticker Features</h3>
                <p><strong>Count:</strong> ${data.stickers_features.stickers_count}</p>
                <p><strong>Total Value:</strong> $${data.stickers_features.stickers_total_value.toFixed(2)}</p>
                <p><strong>Average Value:</strong> $${data.stickers_features.stickers_avg_value.toFixed(2)}</p>
                <p><strong>Max Value:</strong> $${data.stickers_features.stickers_max_value.toFixed(2)}</p>
            `;
        } else {
            stickerFeaturesDiv.innerHTML = '';
        }
        
        // Show result and explain button
        resultDiv.classList.remove('hidden');
        explainBtn.classList.remove('hidden');
        
    } catch (error) {
        showError('Prediction failed: ' + error.message);
        console.error('Prediction error:', error);
    } finally {
        predictBtn.disabled = false;
        predictBtn.textContent = 'Get Price Prediction';
    }
}

// Get AI explanation (step 1)
async function getAiExplanation() {
    if (!lastPredictionData || lastPredictedPrice === null) {
        showError('No prediction data available');
        return;
    }
    
    hideError();
    aiExplanationDiv.classList.add('hidden');
    explainV2Btn.classList.add('hidden');
    aiExplanationV2Div.classList.add('hidden');
    
    try {
        // Build request body for AI explanation
        const requestBody = {
            predictedPrice: lastPredictedPrice,
            skinId: lastPredictionData.skinId,
            wearTierId: lastPredictionData.wearTierId,
            floatValue: lastPredictionData.floatValue,
            isStattrak: lastPredictionData.isStattrak,
            stickers: lastPredictionData.stickers
        };
        
        // Only add pattern if it exists
        if (lastPredictionData.pattern) {
            requestBody.pattern = lastPredictionData.pattern;
        }
        
        console.log('Sending AI explanation request:', requestBody);
        
        explainBtn.disabled = true;
        explainBtn.textContent = 'Loading...';
        
        const response = await fetch(`${API_BASE}/ai/explain`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(requestBody)
        });
        
        if (!response.ok) {
            throw new Error('Failed to get AI explanation');
        }
        
        const data = await response.json();
        console.log('AI explanation response:', data);
        
        aiExplanationDiv.innerHTML = `
            <h3>🤖 AI Explanation</h3>
            <p>${data.explanation}</p>
        `;
        aiExplanationDiv.classList.remove('hidden');
        
        // Show detailed explanation button ONLY after basic explanation succeeds
        explainV2Btn.classList.remove('hidden');
        
    } catch (error) {
        showError('AI explanation failed: ' + error.message);
        console.error('AI explanation error:', error);
        // Don't show V2 button if explanation failed
        explainV2Btn.classList.add('hidden');
    } finally {
        explainBtn.disabled = false;
        explainBtn.textContent = 'Get AI Explanation';
    }
}

// Get detailed AI explanation (step 2)
async function getAiExplanationV2() {
    if (!lastPredictionData || lastPredictedPrice === null) {
        showError('No prediction data available');
        return;
    }
    
    hideError();
    aiExplanationV2Div.classList.add('hidden');
    
    try {
        // Build request body for AI explanation V2
        const requestBody = {
            predictedPrice: lastPredictedPrice,
            skinId: lastPredictionData.skinId,
            wearTierId: lastPredictionData.wearTierId,
            floatValue: lastPredictionData.floatValue,
            isStattrak: lastPredictionData.isStattrak,
            stickers: lastPredictionData.stickers
        };
        
        // Only add pattern if it exists
        if (lastPredictionData.pattern) {
            requestBody.pattern = lastPredictionData.pattern;
        }
        
        console.log('Sending detailed AI explanation request:', requestBody);
        
        explainV2Btn.disabled = true;
        explainV2Btn.textContent = 'Loading...';
        
        const response = await fetch(`${API_BASE}/ai/explain-v2`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(requestBody)
        });
        
        if (!response.ok) {
            throw new Error('Failed to get detailed AI explanation');
        }
        
        const data = await response.json();
        console.log('Detailed AI explanation response:', data);
        
        aiExplanationV2Div.innerHTML = `
            <h3>📚 Detailed AI Explanation</h3>
            <p>${data.explanation}</p>
        `;
        aiExplanationV2Div.classList.remove('hidden');
        
    } catch (error) {
        showError('Detailed explanation failed: ' + error.message);
        console.error('Detailed explanation error:', error);
    } finally {
        explainV2Btn.disabled = false;
        explainV2Btn.textContent = 'Get Detailed AI Explanation';
    }
}

// Event Listeners
weaponTypeSelect.addEventListener('change', (e) => {
    selectedWeaponTypeId = e.target.value;
    if (selectedWeaponTypeId) {
        hideError();
        hideStepsFrom(2);
        resultDiv.classList.add('hidden');
        loadWeapons(selectedWeaponTypeId);
    }
});

weaponSelect.addEventListener('change', (e) => {
    selectedWeaponId = e.target.value;
    if (selectedWeaponId) {
        hideError();
        hideStepsFrom(3);
        resultDiv.classList.add('hidden');
        loadSkins(selectedWeaponId);
    }
});

skinSelect.addEventListener('change', (e) => {
    selectedSkinId = e.target.value;
    if (selectedSkinId) {
        hideError();
        hideStepsFrom(4);
        resultDiv.classList.add('hidden');
        loadWearTiers(selectedSkinId);
        loadPatterns(selectedSkinId);
    }
});

wearTierSelect.addEventListener('change', (e) => {
    selectedWearTierId = e.target.value;
    if (selectedWearTierId) {
        hideError();
        showStep(6); // Show stickers step
        showStep(7); // Show float step
        showStep(8); // Show stattrak step
    }
});

patternSelect.addEventListener('change', (e) => {
    selectedPattern = e.target.value || null;
});

stickerSearchInput.addEventListener('input', (e) => {
    clearTimeout(stickerSearchTimeout);
    const query = e.target.value.trim();
    
    if (query.length === 0) {
        stickerResultsDiv.classList.remove('active');
        return;
    }
    
    stickerSearchTimeout = setTimeout(() => {
        searchStickers(query);
    }, 300);
});

stickerSearchInput.addEventListener('focus', () => {
    if (stickerSearchInput.value.trim().length > 0) {
        searchStickers(stickerSearchInput.value.trim());
    } else {
        searchStickers('');
    }
});

document.addEventListener('click', (e) => {
    if (!e.target.closest('.sticker-search')) {
        stickerResultsDiv.classList.remove('active');
    }
});

predictBtn.addEventListener('click', makePrediction);
explainBtn.addEventListener('click', getAiExplanation);
explainV2Btn.addEventListener('click', getAiExplanationV2);

// Initialize
loadWeaponTypes();
