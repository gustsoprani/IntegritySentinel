flowchart TD
    A[Início do Ciclo (Timer)] --> B{Pasta Existe?}
    B -- Não --> Z[Logar Erro e Aguardar]
    B -- Sim --> C[Listar Arquivos na Pasta]
    C --> D[Para CADA Arquivo]
    
    D --> E{Arquivo Bloqueado?}
    E -- Sim --> F[Logar Warning / Tentar Prox Ciclo]
    E -- Não --> G[Calcular Hash SHA-256]
    
    G --> H{Hash existe no BD?}
    H -- Não (Novo) --> I[INSERT no Banco]
    I --> J[Log: ARQUIVO CRIADO]
    
    H -- Sim --> K{Hash Mudou?}
    K -- Sim (Alterado) --> L[UPDATE no Banco]
    L --> M[Log: ARQUIVO ALTERADO!]
    
    K -- Não (Igual) --> N[Ignorar (Nenhuma Ação)]
    
    J --> O[Próximo Arquivo]
    M --> O
    N --> O
    F --> O
    
    O --> P{Acabaram os Arquivos?}
    P -- Não --> D
    P -- Sim --> Q[Verificar Deletados]
    Q --> R[Dormir (Intervalo Configurado)]
    R --> A