const API_BASE = '/api/wallet';
let selectedWalletId = null;
let selectedCurrency = null;
let allWallets = [];

const currencySymbols = {
    "USD": "$",
    "RUB": "₽",
    "EUR": "€"
}

function showError(message) {
    const existingError = document.querySelector('.error-message');
    if (existingError) existingError.remove();

    const errorDiv = document.createElement('div');
    errorDiv.className = 'error-message';
    errorDiv.textContent = message;
    document.querySelector('.app-container').insertBefore(errorDiv, document.querySelector('.header').nextSibling);

    setTimeout(() => errorDiv.remove(), 5000);
}

function formatCurrency(amount, currency) {
    const symbol = currencySymbols[currency] || '₽';
    return `${amount.toLocaleString('ru-RU', { minimumFractionDigits: 2, maximumFractionDigits: 2 })} ${symbol}`;
}

function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('ru-RU', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
}

async function apiCall(endpoint, options = {}) {
    try {
        const response = await fetch(`${API_BASE}${endpoint}`, {
            headers: {
                'Content-Type': 'application/json',
                ...options.headers
            },
            ...options
        });

        if (!response.ok) {
            const errorData = await response.json().catch(() => ({}));
            throw new Error(errorData.message || `HTTP error! status: ${response.status}`);
        }

        return await response.json();
    } catch (error) {
        console.error('API call failed:', error);
        showError(error.message);
        throw error;
    }
}

async function loadAllData() {
    const month = document.getElementById('monthSelect').value;
    const year = document.getElementById('yearSelect').value;

    if (!month || !year) {
        showError('Пожалуйста, выберите месяц и год');
        return;
    }

    try {
        allWallets = await apiCall(`/get/all/${year}/${month}/3`);

        if (!selectedWalletId && allWallets.length > 0) {
            selectedWalletId = allWallets[0].id;
        }

        displaySelectedWallet();
        displayOtherWallets();
        await loadWalletTransactions();
        updateWalletDropdowns();

    } catch (error) {
        console.error('Failed to load data:', error);
    }
}

async function loadWalletTransactions() {
    if (!selectedWalletId) return;

    const month = document.getElementById('monthSelect').value;
    const year = document.getElementById('yearSelect').value;

    try {
        const transactionsInfo = await apiCall(`/${selectedWalletId}/transactions/${year}/${month}`);
        displayTransactions(transactionsInfo);
        calculateMonthlySummary(transactionsInfo.incomeTransactionsGroup.totalSum, transactionsInfo.expenseTransactionsGroup.totalSum, selectedCurrency);
    } catch (error) {
        console.error('Failed to load transactions:', error);
    }
}

async function displaySelectedWallet() {
    const selectedWallet = allWallets.find(w => w.id === selectedWalletId);
    selectedCurrency = selectedWallet.currency;
    const container = document.getElementById('selectedWalletCard');

    if (!selectedWallet) {
        container.innerHTML = '<div class="empty-state">Кошелёк не найден</div>';
        return;
    }

    let currentBalance = null;

    await apiCall(`/${selectedWalletId}`).then((res) => {
        currentBalance = res.currentBalance;
    });

    container.innerHTML = `
                <div class="wallet-header">
                    <div>
                        <div class="wallet-name">${selectedWallet.walletName}</div>
                        <div class="wallet-currency">${selectedWallet.currency}</div>
                    </div>
                </div>
                <div class="wallet-balance-section">
                    <div class="balance-label">Текущий баланс</div>
                    <div class="current-balance">${formatCurrency(currentBalance, selectedWallet.currency)}</div>
                </div>
                <div class="wallet-stats">
                    <div class="stat-item">
                        <div class="stat-label">Начальный баланс</div>
                        <div class="stat-value">${formatCurrency(selectedWallet.initialBalance, selectedWallet.currency)}</div>
                    </div>
                    <div class="stat-item">
                        <div class="stat-label">Текущий баланс</div>
                        <div class="stat-value">${formatCurrency(currentBalance, selectedWallet.currency)}</div>
                    </div>
                </div>
            `;
}

function displayOtherWallets() {
    const container = document.getElementById('otherWalletsContainer');
    const otherWallets = allWallets.filter(w => w.id !== selectedWalletId);

    if (otherWallets.length === 0) {
        container.innerHTML = '<div class="empty-state">Нет других кошельков</div>';
        return;
    }

    container.innerHTML = otherWallets.map(wallet => `
                <div class="other-wallet-card" onclick="selectWallet('${wallet.id}')">
                    <div class="other-wallet-header">
                        <div class="other-wallet-name">${wallet.walletName}</div>
                        <div class="other-wallet-currency">${wallet.currency}</div>
                    </div>
                    <div class="other-wallet-balance">
                        ${formatCurrency(wallet.currentBalance, wallet.currency)}
                    </div>
                    ${wallet.transactions && wallet.transactions.length > 0 ? `
                        <div class="other-wallet-top-expenses">
                            <div style="font-size: 0.9rem; color: var(--gray); margin-bottom: 12px;">Топ траты:</div>
                            ${wallet.transactions.map(transaction => `
                                <div class="expense-item">
                                    <span title="${transaction.description}">${transaction.description.length > 20 ? transaction.description.substring(0, 20) + '...' : transaction.description}</span>
                                    <span class="expense-amount">-${formatCurrency(transaction.amount, wallet.currency)}</span>
                                </div>
                            `).join('')}
                        </div>
                    ` : '<div style="color: var(--gray); font-size: 0.9rem;">Нет трат за период</div>'}
                </div>
            `).join('');
        }

        function displayTransactions(transactionsInfo) {
            const incomeContainer = document.getElementById('incomeTransactions');
            const expenseContainer = document.getElementById('expenseTransactions');

            if (transactionsInfo.incomeTransactionsGroup.transactions.length > 0) {
                incomeContainer.innerHTML = transactionsInfo.incomeTransactionsGroup.transactions.map(transaction => `
                    <div class="transaction-item">
                        <div class="transaction-info">
                            <div class="transaction-description">${transaction.description}</div>
                            <div class="transaction-date">${formatDate(transaction.date)}</div>
                        </div>
                        <div class="transaction-amount income">+${formatCurrency(transaction.amount, allWallets.find(w => w.id === selectedWalletId).currency)}</div>
                    </div>
                `).join('');
            } else {
                incomeContainer.innerHTML = '<div class="empty-state">Нет доходов за период</div>';
            }

            if (transactionsInfo.expenseTransactionsGroup.transactions.length > 0) {
                expenseContainer.innerHTML = transactionsInfo.expenseTransactionsGroup.transactions.map(transaction => `
                    <div class="transaction-item">
                        <div class="transaction-info">
                            <div class="transaction-description">${transaction.description}</div>
                            <div class="transaction-date">${formatDate(transaction.date)}</div>
                        </div>
                        <div class="transaction-amount expense">-${formatCurrency(transaction.amount, allWallets.find(w => w.id === selectedWalletId).currency)}</div>
                    </div>
                `).join('');
            } else {
                expenseContainer.innerHTML = '<div class="empty-state">Нет расходов за период</div>';
            }
        }

        function updateWalletDropdowns() {
            const senderSelect = document.getElementById('senderWallet');
            const receiverSelect = document.getElementById('receiverWallet');
            
            const options = allWallets.map(wallet => 
                `<option value="${wallet.id}">${wallet.walletName} (${formatCurrency(wallet.currentBalance, wallet.currency)})</option>`
            ).join('');
            
            senderSelect.innerHTML = '<option value="">Выберите кошелёк</option>' + options;
            receiverSelect.innerHTML = '<option value="">Выберите кошелёк</option>' + options;
        }

         async function calculateMonthlySummary(totalIncome, totalExpense, currency) {

            document.getElementById('totalMonthlyIncome').textContent = formatCurrency(totalIncome, currency);
            document.getElementById('totalMonthlyExpense').textContent = formatCurrency(totalExpense, currency);
            document.getElementById('monthlyNet').textContent = formatCurrency(totalIncome - totalExpense, currency);
        }

        function selectWallet(walletId) {
            selectedWalletId = walletId;
            displaySelectedWallet();
            displayOtherWallets();
            loadWalletTransactions();
        }

        async function createTransaction() {
            const senderId = document.getElementById('senderWallet').value;
            const receiverId = document.getElementById('receiverWallet').value;
            const amount = parseFloat(document.getElementById('transactionAmount').value);
            const description = document.getElementById('transactionDescription').value;

            if (!senderId || !receiverId || !amount || amount <= 0) {
                showError('Пожалуйста, заполните все поля корректно');
                return;
            }

            if (senderId === receiverId) {
                showError('Нельзя переводить самому себе');
                return;
            }

            try {
                await apiCall('/transaction/create', {
                    method: 'POST',
                    body: JSON.stringify({
                        senderWalletId: senderId,
                        receiverWalletId: receiverId,
                        amount: amount,
                        description: description
                    })
                });

                document.getElementById('transactionAmount').value = '';
                document.getElementById('transactionDescription').value = '';
                
                loadAllData();
                
            } catch (error) {
                console.error('Failed to create transaction:', error);
            }
        }

        async function createWallet() {
            const name = document.getElementById('newWalletName').value;
            const currency = parseInt(document.getElementById('newWalletCurrency').value);
            const balance = parseFloat(document.getElementById('newWalletBalance').value);

            if (!name || balance < 0) {
                showError('Пожалуйста, заполните все поля корректно');
                return;
            }

            try {
                await apiCall('/create', {
                    method: 'POST',
                    body: JSON.stringify({
                        walletName: name,
                        currency: currency,
                        initialBalance: balance
                    })
                });

                document.getElementById('newWalletName').value = '';
                document.getElementById('newWalletBalance').value = '';
                
                loadAllData();
                
            } catch (error) {
                console.error('Failed to create wallet:', error);
            }
        }

        document.addEventListener('DOMContentLoaded', function() {
            const now = new Date();
            document.getElementById('monthSelect').value = now.getMonth() + 1;
            document.getElementById('yearSelect').value = now.getFullYear();
            
            loadAllData();
        });